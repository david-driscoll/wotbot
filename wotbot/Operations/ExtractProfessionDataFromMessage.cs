using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentValidation;
using MediatR;
using Rocket.Surgery.Encoding;

namespace wotbot.Operations
{
    public static class ExtractProfessionDataFromMessage
    {
        public record Request(string Content) : IRequest<IEnumerable<Response>>;

        public record Item(int Id, string Name);

        public record Response(string Player, string Profession, ImmutableArray<Item> Items);

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.Content).NotNull();
            }
        }

        class Handler : RequestHandler<Request, IEnumerable<Response>>
        {
            protected override IEnumerable<Response> Handle(Request request)
            {
                var players = new Dictionary<(string player, string profession), ImmutableArray<Item>.Builder>();
                foreach (var line in request.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!line.TrimStart().StartsWith("!profession import")) continue;
                    var base64Content = line[19..];
                    var data = Base3264Encoding.DecodeToString(EncodingType.Base64, base64Content);
                    var parts = data.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 3) continue;
                    if (!players.TryGetValue((parts[0], parts[1]), out var builder))
                    {
                        builder = ImmutableArray.CreateBuilder<Item>();
                        players.Add((parts[0], parts[1]), builder);
                    }

                    var items = parts[2].Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var itemString in items)
                    {
                        if (!itemString.Contains('#')) continue;
                        var itemBits = itemString.Split('#');
                        builder.Add(new Item(int.Parse(itemBits[1]), itemBits[0]));
                    }
                }

                return players.Select(z => new Response(z.Key.player, z.Key.profession, z.Value.ToImmutable())).ToArray();
            }
        }
    }
}
