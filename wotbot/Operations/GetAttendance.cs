using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Azure;
using FluentValidation;
using MediatR;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;
using wotbot.Domain;
using wotbot.Infrastructure;
using wotbot.Models;

namespace wotbot.Operations;

public record AttendanceRecord(string Player, double AllTime, double ThirtyDay, double SixtyDay, double NinetyDay);

public static class GetAttendance
{
    public record Request(string TeamId) : IRequest<ImmutableArray<AttendanceRecord>>
    {
        public bool IncludeDeleted { get; init; }
    }

    class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(z => z.TeamId).NotNull().NotEmpty();
        }
    }

    record AttendanceEntry(LocalDate Date, int HitCount);

    class Handler : IRequestHandler<Request, ImmutableArray<AttendanceRecord>>
    {
        private readonly ITableClientFactory _tableClientFactory;
        private readonly IMapper _mapper;

        public Handler(ITableClientFactory tableClientFactory, IMapper mapper)
        {
            _tableClientFactory = tableClientFactory;
            _mapper = mapper;
        }

        public async Task<ImmutableArray<AttendanceRecord>> Handle(Request request, CancellationToken cancellationToken)
        {
            var tableClient = _tableClientFactory.CreateClient(Constants.PointsTable);

            var allRaids = new SortedDictionary<LocalDate, HashSet<string>>(Comparer<LocalDate>.Create((date, localDate) => localDate.CompareTo(date)));
            var playerRaids = new SortedDictionary<string, SortedDictionary<LocalDate, int>>(StringComparer.OrdinalIgnoreCase);

            await foreach (var entry in tableClient.QueryAsync<AwardedPointsTableEntity>($"(PartitionKey eq '{request.TeamId}')", cancellationToken: cancellationToken))
            {
                if (entry.Reason == "Weekly Decay") continue;
                var zonedDateTime = Instant.FromDateTimeOffset(entry.Date).InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/Los_Angeles") ?? DateTimeZone.Utc);
                if (!allRaids.TryGetValue(zonedDateTime.Date, out var set))
                {
                    set = allRaids[zonedDateTime.Date] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                set.Add(entry.Player);

                if (!playerRaids.TryGetValue(entry.Player, out var raidAttendance))
                {
                    raidAttendance = playerRaids[entry.Player] = new SortedDictionary<LocalDate, int>(Comparer<LocalDate>.Create((date, localDate) => localDate.CompareTo(date)));
                }

                if (raidAttendance.ContainsKey(zonedDateTime.Date))
                {
                    raidAttendance[zonedDateTime.Date] += 1;
                }
                else
                {
                    raidAttendance[zonedDateTime.Date] = 1;
                }
            }

            var removeRaids = allRaids.Where(z => z.Value.Count < 17).Select(z => z.Key).ToArray();
            foreach (var raid in removeRaids)
            {
                allRaids.Remove(raid);
            }
            foreach (var player in playerRaids)
            {
                foreach (var raid in removeRaids)
                {
                    player.Value.Remove(raid);
                }
            }

            var lastRaid = allRaids.First().Key;
            var thirtyDays = lastRaid.Minus(Period.FromDays(30));
            var sixtyDays = lastRaid.Minus(Period.FromDays(60));
            var nintyDays = lastRaid.Minus(Period.FromDays(90));
            var raidsLast30 = allRaids.Keys.TakeWhile(z => z >= thirtyDays).ToArray();
            var raidsLast60 = allRaids.Keys.TakeWhile(z => z >= sixtyDays).ToArray();
            var raidsLast90 = allRaids.Keys.TakeWhile(z => z >= nintyDays).ToArray();

            var playerAttendance = new SortedDictionary<string, AttendanceRecord>(StringComparer.OrdinalIgnoreCase);

            foreach (var (player, value) in playerRaids)
            {
                var percent30 = value.Keys.TakeWhile(z => z >= thirtyDays).Count()/ (double)raidsLast30.Length;
                var percent60 = value.Keys.TakeWhile(z => z >= sixtyDays).Count()/ (double)raidsLast60.Length;
                var percent90 = value.Keys.TakeWhile(z => z >= nintyDays).Count()/ (double)raidsLast90.Length;
                var allTimePercent = 0d;
                var firstRaid = value.Keys.LastOrDefault();
                if (firstRaid != default)
                {
                    var allPlayerRaids = allRaids.Keys.TakeWhile(z => z >= firstRaid).Count();
                    var alltime = value.Count;
                    allTimePercent = alltime / (double)allPlayerRaids;
                }
                var record = new AttendanceRecord(player, allTimePercent, percent30, percent60, percent90);
                playerAttendance.Add(player, record);
            }

            return playerAttendance.Values.ToImmutableArray();
        }
    }
}
