using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Azure;
using FluentValidation;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;
using wotbot.Domain;
using wotbot.Infrastructure;
using wotbot.Models;

namespace wotbot.Operations
{
    public static class ListProfiles
    {
        public record Request(string TeamId) : IRequest<ImmutableArray<PlayerProfile>>
        {
            public WarcraftClass? Class { get; init; }
            public WarcraftRole? Role { get; init; }
            public bool IncludeDeleted { get; init; }
        }

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.TeamId).NotNull().NotEmpty();
            }
        }

        class Handler : IRequestHandler<Request, ImmutableArray<PlayerProfile>>
        {
            private readonly ITableClientFactory _tableClientFactory;
            private readonly IMapper _mapper;

            public Handler(ITableClientFactory tableClientFactory, IMapper mapper)
            {
                _tableClientFactory = tableClientFactory;
                _mapper = mapper;
            }

            public async Task<ImmutableArray<PlayerProfile>> Handle(Request request, CancellationToken cancellationToken)
            {
                var tableClient = _tableClientFactory.CreateClient(Constants.ProfilesTable);
                try
                {
                    AsyncPageable<PlayerProfileTableEntity> result;
                    if (request.Class.HasValue)
                    {
                        var normal = request.Class.ToString();
                        var lower = request.Class.ToString()!.ToLower();
                        var upper = request.Class.ToString()!.ToUpper();
                        result = tableClient.QueryAsync<PlayerProfileTableEntity>(x => x.TeamId == request.TeamId && (x.Class == normal || x.Class == lower || x.Class == upper),
                            cancellationToken: cancellationToken);
                    }
                    else if (request.Role.HasValue)
                    {
                        var normal = request.Role.ToString();
                        var lower = request.Role.ToString()!.ToLower();
                        var upper = request.Role.ToString()!.ToUpper();
                        result = tableClient.QueryAsync<PlayerProfileTableEntity>(x => x.TeamId == request.TeamId && (x.Role == normal || x.Role == lower || x.Role == upper),
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        result = tableClient.QueryAsync<PlayerProfileTableEntity>($"(PartitionKey eq '{request.TeamId}')", cancellationToken: cancellationToken);
                    }

                    return (await result.Where(z => request.IncludeDeleted || z.Deleted == false).Select(_mapper.Map<PlayerProfile>).ToArrayAsync(cancellationToken)).ToImmutableArray();
                }
                catch (Azure.RequestFailedException)
                {
                    throw new NotFoundException();
                }
            }
        }
    }
}
