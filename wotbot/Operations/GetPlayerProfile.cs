using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using ImTools;
using MediatR;
using Rocket.Surgery.Encoding;
using Rocket.Surgery.LaunchPad.Foundation;
using wotbot.Domain;
using wotbot.Infrastructure;
using wotbot.Models;

namespace wotbot.Operations
{
    public static class GetPlayerProfile
    {
        public record Request(string TeamId, string Name) : IRequest<PlayerProfile>;

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.TeamId).NotNull().NotEmpty();
                RuleFor(z => z.Name).NotNull().NotEmpty();
            }
        }

        class Handler : IRequestHandler<Request, PlayerProfile>
        {
            private readonly ITableClientFactory _tableClientFactory;
            private readonly IMapper _mapper;

            public Handler(ITableClientFactory tableClientFactory, IMapper mapper)
            {
                _tableClientFactory = tableClientFactory;
                _mapper = mapper;
            }

            public async Task<PlayerProfile> Handle(Request request, CancellationToken cancellationToken)
            {
                var tableClient = _tableClientFactory.CreateClient(Constants.ProfilesTable);
                try
                {
                    var entity = await tableClient.GetEntityAsync<PlayerProfileTableEntity>(request.TeamId, Base3264Encoding.ToBase32Crockford(request.Name.ToLowerInvariant()),
                        cancellationToken: cancellationToken);
                    return _mapper.Map<PlayerProfile>(entity.Value);
                }
                catch (Azure.RequestFailedException)
                {
                    throw new NotFoundException();
                }
            }
        }
    }
}
