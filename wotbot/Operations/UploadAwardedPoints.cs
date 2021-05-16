using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Data.Tables;
using FluentValidation;
using MediatR;
using wotbot.Domain;
using wotbot.Infrastructure;
using wotbot.Models;

namespace wotbot.Operations
{
    public static class UploadAwardedPoints
    {
        public record Request(TeamRecord Team, ImmutableArray<AwardedPoints> AwardedPoints) : IRequest<ImmutableArray<AwardedPoints>>;

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.Team).NotNull();
                RuleFor(z => z.AwardedPoints).NotNull();
            }
        }

        class Mapper : Profile
        {
            public Mapper()
            {
                CreateMap<AwardedPoints, AwardedPointsTableEntity>()
                    .ForMember(z => z.TeamId, z => z.Ignore())
                    .ReverseMap();
            }
        }

        class Handler : IRequestHandler<Request, ImmutableArray<AwardedPoints>>
        {
            private readonly ITableClientFactory _tableClientFactory;
            private readonly IMapper _mapper;

            public Handler(ITableClientFactory tableClientFactory, IMapper mapper)
            {
                _tableClientFactory = tableClientFactory;
                _mapper = mapper;
            }

            public async Task<ImmutableArray<AwardedPoints>> Handle(Request request, CancellationToken cancellationToken)
            {
                var tableClient = _tableClientFactory.CreateClient(Constants.PointsTable);
                await tableClient.CreateIfNotExistsAsync(cancellationToken);

                var points =
                    request.AwardedPoints
                        .Select(profile =>
                        {
                            var result = _mapper.Map<AwardedPointsTableEntity>(profile);
                            result.TeamId = request.Team.TeamId;
                            return result;
                        })
                        .ToArray();
                if (points.Any())
                {
                    var hightestDate = points.Max(z => z.Date);
                    var lowestDate = points.Min(z => z.Date);
                    var allItems = await tableClient
                        .QueryAsync<AwardedPointsTableEntity>(entity => entity.PartitionKey == request.Team.TeamId && entity.Date >= lowestDate && entity.Date <= hightestDate)
                        .ToArrayAsync(cancellationToken);

                    var newItems = points.Except(allItems).ToArray();

                    foreach (var buffer in points
                        .Except(allItems)
                        .Select(profile => new TableTransactionAction(TableTransactionActionType.Add, profile))
                        .Buffer(100)
                    )
                    {
                        if (!buffer.Any()) continue;
                        await tableClient.SubmitTransactionAsync(buffer, cancellationToken);
                    }

                    return newItems.Select(_mapper.Map<AwardedPoints>).ToImmutableArray();
                }

                return ImmutableArray<AwardedPoints>.Empty;
            }
        }
    }
}
