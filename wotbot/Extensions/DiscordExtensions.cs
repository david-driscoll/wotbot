using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using Emzi0767.Utilities;
using wotbot.Models;
using wotbot.Operations;

// ReSharper disable once CheckNamespace
namespace wotbot
{
    public static class DiscordExtensions
    {
        public delegate IObservable<TR> DiscordReactiveEventHandler<in T, TR>(DiscordClient discordClient, T args) where T : AsyncEventArgs;

        public static IObservable<R> FromDiscordEvent<T, R>(
            Action<AsyncEventHandler<DiscordClient, T>> subscribe,
            Action<AsyncEventHandler<DiscordClient, T>> unsubscribe,
            DiscordReactiveEventHandler<T, R> transform
        ) where T : AsyncEventArgs
        {
            return Observable.Create<R>(observer =>
            {
                AsyncEventHandler<DiscordClient, T> method = (client, arg2) => transform(client, arg2).ToTask();
                subscribe(method);
                return Disposable.Create(() => { unsubscribe(method); });
            });
        }

        public static IObservable<T> FromDiscordEvent<T>(
            Action<AsyncEventHandler<DiscordClient, T>> subscribe,
            Action<AsyncEventHandler<DiscordClient, T>> unsubscribe,
            DiscordReactiveEventHandler<T, T> transform
        ) where T : AsyncEventArgs
        {
            return FromDiscordEvent<T, T>(subscribe, unsubscribe, transform);
        }

        public static IObservable<T> FromDiscordEvent<T>(
            Action<AsyncEventHandler<DiscordClient, T>> subscribe,
            Action<AsyncEventHandler<DiscordClient, T>> unsubscribe
        ) where T : AsyncEventArgs
        {
            return FromDiscordEvent<T, T>(subscribe, unsubscribe, (client, args) => Observable.Return(args));
        }

        public static DiscordMessageBuilder AddStandings(this DiscordMessageBuilder builder, string title, DiscordGuild guild, IEnumerable<PlayerProfile> profile,
            ImmutableArray<AttendanceRecord> attendanceRecords)
        {
            DiscordEmbedBuilder embed = null;

            var attendanceData = attendanceRecords.ToDictionary(z => z.Player, StringComparer.OrdinalIgnoreCase);
            var showAttendance = attendanceData.Any();
            foreach (var embedContent in profile
                         .OrderByDescending(z => z.CurrentPoints)
                         .Where(z => z.CurrentPoints != 0)
                         .Buffer(10)
                         .Select((z, i) => (list: z, index: i))
                         .Buffer(25/3)
                    )
            {
                embed ??= new DiscordEmbedBuilder().WithTitle(title);
                foreach (var (list, index) in embedContent)
                {
                    var pointsLine = new StringBuilder();
                    var playerLine = new StringBuilder();
                    var attendanceLine = new StringBuilder();
                    foreach (var item in list)
                    {
                        // :100: :high_brightness: :warning: :interrobang: :grey_question:
                        var emoji = guild.Emojis.Values.FirstOrDefault(z => z.Name.Contains(item.GetClass().ToString(), StringComparison.OrdinalIgnoreCase))?.ToString() ??
                                    $"[{item.GetClass().ToString().ToLowerInvariant()}]";

                        pointsLine.AppendLine($"`{item.CurrentPoints,4:D}`:moneybag:");
                        playerLine.AppendLine($"{emoji}`{item.Player,-12}`");

                        if (showAttendance)
                        {
                            var attendance30DayText = ":grey_question:";
                            var attendanceAllTimeText = ":grey_question:";
                            if (attendanceData.TryGetValue(item.Player, out var playerAttendance))
                            {
                                attendance30DayText = GetAttendanceString(playerAttendance.ThirtyDay);
                                attendanceAllTimeText = GetAttendanceString(playerAttendance.AllTime);
                            }

                            attendanceLine.AppendLine($"{attendance30DayText} | {attendanceAllTimeText}");
                        }
                    }

                    var pointsSpanStart = list.First().CurrentPoints.ToString("D0");
                    var pointsSpanEnd = list.Last().CurrentPoints.ToString("D0");

                    embed.AddField("\u3000" + pointsSpanStart + "\u3000", pointsLine.ToString(), inline: true);
                    embed.AddField("\u3000:left_right_arrow:\u3000", playerLine.ToString(), inline: true);
                    if (showAttendance)
                    {
                        embed.AddField("\u3000" + pointsSpanEnd + "\u3000", attendanceLine.ToString(), inline: true);
                    }
                    else
                    {
                        embed.AddField("\u3000" + pointsSpanEnd + "\u3000", "--", inline: true);
                    }
                }

                // embed.WithDescription(string.Join("", "-".Repeat(80)));
                builder.AddEmbed(embed);
                embed = new DiscordEmbedBuilder();
            }

            return builder;

            static string GetAttendanceString(double value)
            {
                if ((int)value >= 1) return $"`{value,4:P0}`:robot:";
                if (value > 0.79) return $"`{value,4:P0}`:face_with_monocle:";
                if (value > 0.69) return $"`{value,4:P0}`:grinning:";
                if (value > 0.49) return $"`{value,4:P0}`:anguished:";
                return $"`{value,4:P0}`:sleeping:";
            }
        }
    }
}
