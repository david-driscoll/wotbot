using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using FluentValidation;
using MediatR;
using NLua;
using Rocket.Surgery.LaunchPad.Foundation;
using wotbot.Infrastructure;
using wotbot.Models;

namespace wotbot.Operations
{
    public static class ExtractDataFromSavedVariables
    {
        public record Request(
            string StorageContainerName,
            string BlobPath
        ) : IRequest<IImmutableDictionary<TeamRecord, Response>>
        {
            public string DatabaseTableName { get; init; } = "CommDKP_DB";
            public string LootTableName { get; init; } = "CommDKP_Loot";
            public string ProfileTableName { get; init; } = "CommDKP_DKPTable";
            public string HistoryTableName { get; init; } = "CommDKP_DKPHistory";
        }

        public record Response(ImmutableArray<PlayerProfile> PlayerProfiles, ImmutableArray<AwardedLoot> AwardedLoot, ImmutableArray<AwardedPoints> AwardedPoints);

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.StorageContainerName).NotNull().NotEmpty();
                RuleFor(z => z.BlobPath).NotNull().NotEmpty();
                RuleFor(z => z.DatabaseTableName).NotNull().NotEmpty();
                RuleFor(z => z.LootTableName).NotNull().NotEmpty();
                RuleFor(z => z.ProfileTableName).NotNull().NotEmpty();
                RuleFor(z => z.HistoryTableName).NotNull().NotEmpty();
            }
        }

        class Handler : IRequestHandler<Request, IImmutableDictionary<TeamRecord, Response>>
        {
            private readonly IBlobContainerClientFactory _clientFactory;

            public Handler(IBlobContainerClientFactory clientFactory)
            {
                _clientFactory = clientFactory;
            }

            public async Task<IImmutableDictionary<TeamRecord, Response>> Handle(Request request, CancellationToken cancellationToken)
            {
                using var lua = new Lua();
                var blobClient = _clientFactory.CreateClient(request.StorageContainerName).GetBlobClient(request.BlobPath);

                if (!await blobClient.ExistsAsync(cancellationToken))
                {
                    throw new NotFoundException($"Could not find blob at path {request.BlobPath}");
                }

                using var streamReader = new StreamReader(await blobClient.OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken));
                var variablesString = await streamReader.ReadToEndAsync();
                lua.DoString(variablesString);

                var teams = GetAllTeams(request, lua).ToImmutableHashSet();

                var result = teams.ToImmutableDictionary(z => z,
                    z => new Response(ImmutableArray<PlayerProfile>.Empty, ImmutableArray<AwardedLoot>.Empty, ImmutableArray<AwardedPoints>.Empty)).ToBuilder();
                foreach (var (team, profiles) in ExtractProfiles(request, lua, teams))
                {
                    if (!result.TryGetValue(team, out var response)) continue;
                    result[team] = response with {PlayerProfiles = profiles.ToImmutableArray()};
                }

                foreach (var (team, loot) in ExtractAwardedLoot(request, lua, teams))
                {
                    if (!result.TryGetValue(team, out var response)) continue;
                    result[team] = response with {AwardedLoot = loot.ToImmutableArray()};
                }

                foreach (var (team, history) in ExtractRawHistory(request, lua, teams))
                {
                    if (!result.TryGetValue(team, out var response)) continue;
                    result[team] = response with {AwardedPoints = history.ToImmutableArray()};
                }

                return result.ToImmutable();
            }

            private static IEnumerable<(TeamRecord team, IEnumerable<AwardedPoints> awardedPoints)> ExtractRawHistory(Request request, Lua lua, ImmutableHashSet<TeamRecord> teams)
            {
                if (lua.GetObjectFromPath(request.HistoryTableName) is not LuaTable historyTable) throw new Exception("Table not found");
                foreach (var (team, data) in ResolveTeamBasedData<IEnumerable<RawHistoryRecord>>(historyTable, teams))
                {
                    yield return (team, data.SelectMany(RawHistoryRecord.Expand));
                }
            }

            private static IEnumerable<(TeamRecord team, IEnumerable<AwardedLoot> awarededLoot)> ExtractAwardedLoot(Request request, Lua lua, ImmutableHashSet<TeamRecord> teams)
            {
                if (lua.GetObjectFromPath(request.LootTableName) is not LuaTable lootTable) throw new Exception("Table not found");

                return ResolveTeamBasedData<IEnumerable<AwardedLoot>>(lootTable, teams);
            }

            private static IEnumerable<(TeamRecord team, IEnumerable<PlayerProfile> profiles)> ExtractProfiles(Request request, Lua lua, ImmutableHashSet<TeamRecord> teams)
            {
                if (lua.GetObjectFromPath(request.ProfileTableName) is not LuaTable dkpTable) throw new Exception("Table not found");

                return ResolveTeamBasedData<IEnumerable<PlayerProfile>>(dkpTable, teams);
            }

            private static IEnumerable<TeamRecord> GetAllTeams(Request request, Lua lua)
            {
                if (lua.GetObjectFromPath(request.DatabaseTableName) is not LuaTable dbTable) throw new Exception("Table not found");

                var teams = new Dictionary<TeamRecord, string>();
                foreach (var key in dbTable.GetValidKeys())
                {
                    if (dbTable[key] is not LuaTable guilds) continue;
                    foreach (var guild in guilds.GetValidKeys())
                    {
                        if (guilds[guild] is not LuaTable data) continue;
                        if (data.LuaTableToObject() is not IDictionary<object, object> dataDic) continue;
                        if (!dataDic.TryGetValue("teams", out var t)) continue;
                        if (t is not IDictionary<object, object> teamData) continue;

                        foreach (var team in teamData)
                        {
                            if (team.Value is not IDictionary<object, object> td) continue;
                            var teamSplit = key.ToString()!.Split('-', StringSplitOptions.RemoveEmptyEntries);
                            yield return new(teamSplit[0], teamSplit[1], guild.ToString()!, team.Key.ToString()!, td["name"].ToString()!);
                        }
                    }
                }
            }

            private static IEnumerable<(TeamRecord, T)> ResolveTeamBasedData<T>(LuaTable table, ImmutableHashSet<TeamRecord> teams)
            {
                foreach (var key in table.GetValidKeys())
                {
                    if (table[key] is not LuaTable guilds) continue;
                    foreach (var guild in guilds.GetValidKeys())
                    {
                        if (guilds[guild] is not LuaTable data) continue;
                        if (data.LuaTableToObject() is not IDictionary<object, object> dataDic) continue;

                        foreach (var item in dataDic)
                        {
                            // these records will always be zero indexed
                            // and always have a number
                            if (item.Key is string s && !long.TryParse(s, out _)) throw new Exception("unexpected record found");
                            var lookup = new TeamLookup(key.ToString()!, guild.ToString()!, item.Key.ToString()!);
                            var team = teams.First(z => z.TeamId == lookup.ToTeamId());

                            if (item.Value is not T v)
                            {
                                // could be an object or an array we don't really know, just drop it?
                                if (item.Value is IDictionary<object, object> d && d.Count == 0)
                                {
                                    continue;
                                }

                                yield return new(team, JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(item.Value), new JsonSerializerOptions()
                                {
                                    PropertyNameCaseInsensitive = true
                                })!);
                            }
                            else
                            {
                                yield return new(team, v);
                            }
                        }
                    }
                }
            }

            record TeamLookup(string Server, string Guild, string Index)
            {
                public string ToTeamId() => $"{Server}-{Guild}-{Index}";
            }

            record RawHistoryRecord(
                string Players,
                string Index,
                [property: JsonPropertyName("dkp")] long Points,
                [property: JsonConverter(typeof(DateSecondsToDateTimeOffsetJsonConverter))]
                DateTimeOffset Date,
                string Reason
            )
            {
                public static IEnumerable<AwardedPoints> Expand(RawHistoryRecord historyRecord)
                {
                    return historyRecord.Players.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(player => new AwardedPoints(player, historyRecord.Index, historyRecord.Points, historyRecord.Date, historyRecord.Reason));
                }
            };
        }
    }
}
