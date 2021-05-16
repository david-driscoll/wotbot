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
    public static class UploadTeams
    {
        public record Request(ImmutableArray<TeamRecord> Teams) : IRequest;

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.Teams).NotNull();
            }
        }

        class Mapper : Profile
        {
            public Mapper()
            {
                CreateMap<TeamRecord, TeamRecordTableEntity>()
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
                var tableClient = _tableClientFactory.CreateClient(Constants.TeamsTable);
                await tableClient.CreateIfNotExistsAsync(cancellationToken);

                var teams =
                    request.Teams
                        .Select(profile =>
                        {
                            var result = _mapper.Map<TeamRecordTableEntity>(profile);
                            return result;
                        });
                foreach (var team in teams)
                {
                    await tableClient.UpsertEntityAsync(team, TableUpdateMode.Replace, cancellationToken);
                }
                return Unit.Value;
            }
        }
    }
}
