using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using Emzi0767.Utilities;
using wotbot.Models;

// ReSharper disable once CheckNamespace
namespace wotbot
{
    public static class DiscordExtensions
    {
        public delegate IObservable<TR> DiscordReactiveEventHandler<in T, TR>(DiscordClient discordClient, T args) where T :AsyncEventArgs;

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
                return Disposable.Create(() =>
                {
                    unsubscribe(method);
                });

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

        public static DiscordEmbedBuilder AddStandings(this DiscordEmbedBuilder embed, IEnumerable<PlayerProfile> profile)
        {
            foreach (var (list, index) in profile
                .OrderByDescending(z => z.CurrentPoints)
                .Buffer(10)
                .Select((z, i) => (z, i))
            )
            {
                var value = new StringBuilder();
                foreach (var item in list)
                {
                    value.Append($"`{item.CurrentPoints.ToString("D").PadLeft(4)}` :{item.GetClass()}: {item.Player}\n");
                }

                embed.AddField($"{index + 1}-{index + list.Count}", value.ToString());
            }

            return embed;
        }
    }
}
