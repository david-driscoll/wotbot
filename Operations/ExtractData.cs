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
        ) : IRequest<IImmutableDictionary<string, Response>>
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
            }
        }

        class Handler : IRequestHandler<Request, IImmutableDictionary<string, Response>>
        {
            private readonly IBlobContainerClientFactory _clientFactory;

            public Handler(IBlobContainerClientFactory clientFactory)
            {
                _clientFactory = clientFactory;
            }

            public async Task<IImmutableDictionary<string, Response>> Handle(Request request, CancellationToken cancellationToken)
            {
                using var lua = new Lua();
                var blobClient = _clientFactory.CreateClient(request.StorageContainerName).GetBlobClient(request.BlobPath);

                if (!await blobClient.ExistsAsync(cancellationToken))
                {
                    throw new NotFoundException($"Could not find blob at path {request.BlobPath}");
                }

                using var streamReader = new StreamReader(await blobClient.OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken));
                lua.DoString(await streamReader.ReadToEndAsync());

                var teams = GetAllTeams(request, lua);

                var result = teams.ToImmutableDictionary(z => z.Key.ToTeamId(), z => new Response(ImmutableArray<PlayerProfile>.Empty, ImmutableArray<AwardedLoot>.Empty, ImmutableArray<AwardedPoints>.Empty)).ToBuilder();
                foreach (var (team, profiles) in ExtractProfiles(request, lua, teams))
                {
                    var teamId = team.ToTeamId();
                    if (!result.TryGetValue(teamId, out var response)) continue;
                    result[teamId] = response with {PlayerProfiles = profiles.ToImmutableArray() };
                }
                foreach (var (team, loot) in ExtractAwardedLoot(request, lua, teams))
                {
                    var teamId = team.ToTeamId();
                    if (!result.TryGetValue(teamId, out var response)) continue;
                    result[teamId] = response with {AwardedLoot = loot.ToImmutableArray() };
                }
                foreach (var (team, history) in ExtractRawHistory(request, lua, teams))
                {
                    var teamId = team.ToTeamId();
                    if (!result.TryGetValue(teamId, out var response)) continue;
                    result[teamId] = response with {AwardedPoints = history.ToImmutableArray() };
                }

                return result.ToImmutable();
            }

            private static IEnumerable<(TeamRecord team, IEnumerable<AwardedPoints> awardedPoints)> ExtractRawHistory(Request request, Lua lua, Dictionary<TeamLookup, string> teams)
            {
                if (lua.GetObjectFromPath(request.HistoryTableName) is not LuaTable historyTable) throw new Exception("Table not found");
                foreach (var (team, data) in ResolveTeamBasedData<IEnumerable<RawHistoryRecord>>(historyTable, teams))
                {
                    yield return (team, data.SelectMany(RawHistoryRecord.Expand));
                }
            }

            private static IEnumerable<(TeamRecord team, IEnumerable<AwardedLoot> awarededLoot)> ExtractAwardedLoot(Request request, Lua lua, Dictionary<TeamLookup, string> teams)
            {
                if (lua.GetObjectFromPath(request.LootTableName) is not LuaTable lootTable) throw new Exception("Table not found");

                return ResolveTeamBasedData<IEnumerable<AwardedLoot>>(lootTable, teams);
            }

            private static IEnumerable<(TeamRecord team, IEnumerable<PlayerProfile> profiles)> ExtractProfiles(Request request, Lua lua, Dictionary<TeamLookup, string> teams)
            {
                if (lua.GetObjectFromPath(request.ProfileTableName) is not LuaTable dkpTable) throw new Exception("Table not found");

                return ResolveTeamBasedData<IEnumerable<PlayerProfile>>(dkpTable, teams);
            }

            private static Dictionary<TeamLookup, string> GetAllTeams(Request request, Lua lua)
            {
                if (lua.GetObjectFromPath(request.DatabaseTableName) is not LuaTable dbTable) throw new Exception("Table not found");

                var teams = new Dictionary<TeamLookup, string>();
                foreach (var key in dbTable.GetValidKeys())
                {
                    if (dbTable[key] is not LuaTable guilds) continue;
                    foreach (var guild in dbTable.GetValidKeys())
                    {
                        if (guilds[guild] is not LuaTable data) continue;
                        if (data.LuaTableToObject() is not IDictionary<object, object> dataDic) continue;
                        if (!dataDic.TryGetValue("teams", out var t)) continue;
                        if (t is not IDictionary<object, object> teamData) continue;

                        foreach (var team in teamData)
                        {
                            if (team.Value is not IDictionary<object, object> td) continue;
                            teams.Add(new(key.ToString()!, guild.ToString()!, team.Key.ToString()!), td["name"].ToString()!);
                        }
                    }
                }

                return teams;
            }

            private static IEnumerable<(TeamRecord, T)> ResolveTeamBasedData<T>(LuaTable table, IDictionary<TeamLookup, string> teamNames)
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
                            if (!teamNames.TryGetValue(lookup, out var teamName))
                            {
                                teamName = $"UNKNOWN {lookup.Index}";
                            }
                            var teamKey = new TeamRecord(lookup, teamName);
                            if (item.Value is not T v)
                            {
                                // could be an object or an array we don't really know, just drop it?
                                if (item.Value is IDictionary<object, object> d && d.Count == 0)
                                {
                                    continue;
                                }

                                yield return new(teamKey, JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(item.Value), new JsonSerializerOptions()
                                {
                                    PropertyNameCaseInsensitive = true
                                })!);
                            }
                            else
                            {
                                yield return new(teamKey, v);
                            }
                        }
                    }
                }
            }

            record TeamLookup(string Server, string Guild, string Index)
            {
                public string ToTeamId() => $"{Server}-{Guild}-{Index}";
            }

            record TeamRecord(TeamLookup Lookup, string Name) : TeamLookup(Lookup.Server, Lookup.Guild, Lookup.Index);

            record RawHistoryRecord(
                string Players,
                string Index,
                [property: JsonPropertyName("dkp")] long Points,
                long Date,
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
