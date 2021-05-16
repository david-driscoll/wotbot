using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Azure;
using Azure.Data.Tables;
using FluentValidation;
using MediatR;
using wotbot.Domain;
using wotbot.Infrastructure;
using wotbot.Models;

namespace wotbot.Operations
{
    public static class UploadAwardedLoot
    {
        public record Request(TeamRecord Team, ImmutableArray<AwardedLoot> AwardedLoot) : IRequest<ImmutableArray<AwardedLoot>>;

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.Team).NotNull();
                RuleFor(z => z.AwardedLoot).NotNull();
            }
        }

        class Mapper : Profile
        {
            public Mapper()
            {
                CreateMap<AwardedLoot, AwardedLootTableEntity>()
                    .ForMember(z => z.TeamId, z => z.Ignore())
                    .ReverseMap();
            }
        }

        class Handler : IRequestHandler<Request, ImmutableArray<AwardedLoot>>
        {
            private readonly ITableClientFactory _tableClientFactory;
            private readonly IMapper _mapper;

            public Handler(ITableClientFactory tableClientFactory, IMapper mapper)
            {
                _tableClientFactory = tableClientFactory;
                _mapper = mapper;
            }

            public async Task<ImmutableArray<AwardedLoot>> Handle(Request request, CancellationToken cancellationToken)
            {
                var tableClient = _tableClientFactory.CreateClient(Constants.LootTable);
                await tableClient.CreateIfNotExistsAsync(cancellationToken);

                var loot =
                    request.AwardedLoot
                        .Select(profile =>
                        {
                            var result = _mapper.Map<AwardedLootTableEntity>(profile);
                            result.TeamId = request.Team.TeamId;
                            return result;
                        })
                        .ToArray();
                if (loot.Any())
                {
                    var hightestDate = loot.Max(z => z.Date);
                    var lowestDate = loot.Min(z => z.Date);
                    var allItems = await tableClient
                        .QueryAsync<AwardedLootTableEntity>(entity => entity.PartitionKey == request.Team.TeamId && entity.Date >= lowestDate && entity.Date <= hightestDate)
                        .ToArrayAsync(cancellationToken);

                    var newItems = loot.Except(allItems);

                    foreach (var buffer in newItems
                        .Select(profile => new TableTransactionAction(TableTransactionActionType.Add, profile))
                        .Buffer(100)
                    )
                    {
                        if (!buffer.Any()) continue;
                        await tableClient.SubmitTransactionAsync(buffer, cancellationToken);
                    }
                    return newItems.Select(_mapper.Map<AwardedLoot>).ToImmutableArray();
                }

                return ImmutableArray<AwardedLoot>.Empty;
            }
        }
    }
}
