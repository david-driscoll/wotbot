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
    public static class UploadPlayerProfiles
    {
        public record Request(TeamRecord Team, ImmutableArray<PlayerProfile> PlayerProfiles) : IRequest;

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.Team).NotNull();
                RuleFor(z => z.PlayerProfiles).NotNull();
            }
        }

        class Mapper : Profile
        {
            public Mapper()
            {
                CreateMap<PlayerProfile, PlayerProfileTableEntity>()
                    .ForMember(z => z.TeamId, z => z.Ignore())
                    .ForMember(z => z.PlayerLower, z => z.MapFrom(p => p.Player.ToLowerInvariant()))
                    .ReverseMap();
            }
        }

        class Handler : IRequestHandler<Request>
        {
            private readonly ITableClientFactory _tableClientFactory;
            private readonly IMapper _mapper;

            public Handler(ITableClientFactory tableClientFactory, IMapper mapper)
            {
                _tableClientFactory = tableClientFactory;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var tableClient = _tableClientFactory.CreateClient(Constants.ProfilesTable);
                await tableClient.CreateIfNotExistsAsync(cancellationToken);

                var existingProfiles = await tableClient
                    .QueryAsync<PlayerProfileTableEntity>($"(PartitionKey eq '{request.Team.TeamId}')")
                    .ToDictionaryAsync(z => z.RowKey, cancellationToken);

                var profiles =
                    request.PlayerProfiles
                        .Select(profile =>
                        {
                            var result = _mapper.Map<PlayerProfileTableEntity>(profile);
                            result.TeamId = request.Team.TeamId;
                            if (existingProfiles.TryGetValue(result.RowKey, out var existingProfile))
                            {
                                result.Deleted = existingProfile.Deleted;
                            }

                            return result;
                        })
                        .DistinctBy(z => z.RowKey)
                        .ToArray();

                var transactions = new List<List<TableTransactionAction>>
                {
                    new(existingProfiles.Keys
                        .Except(profiles.Select(z => z.RowKey))
                        .Select(profile =>
                        {
                            existingProfiles[profile].Deleted = true;
                            return new TableTransactionAction(TableTransactionActionType.UpsertReplace,
                                existingProfiles[profile]);
                        })
                    ),
                    new(profiles
                        .Select(profile =>
                            new TableTransactionAction(TableTransactionActionType.UpsertReplace, profile))
                        .ToArray()
                    )
                };

                foreach (var buffer in transactions
                             .Where(transaction => transaction.Any())
                             .SelectMany(transaction => transaction.Buffer(100))
                        )
                {
                    await tableClient.SubmitTransactionAsync(buffer, cancellationToken);
                }

                return Unit.Value;
            }
        }
    }
}
