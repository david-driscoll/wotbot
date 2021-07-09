using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;
using wotbot.Domain;
using wotbot.Infrastructure;

namespace wotbot.Operations
{
    public static class FindCrafters
    {
        public record Request(int ItemId) : IRequest<IEnumerable<string>>;

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.ItemId).GreaterThan(0);
            }
        }

        class Handler : IRequestHandler<Request, IEnumerable<string>>
        {
            private readonly ITableClientFactory _tableClientFactory;
            private readonly IMapper _mapper;

            public Handler(ITableClientFactory tableClientFactory, IMapper mapper)
            {
                _tableClientFactory = tableClientFactory;
                _mapper = mapper;
            }
            public async Task<IEnumerable<string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var tableClient = _tableClientFactory.CreateClient(Constants.ProfessionsTable);
                try
                {
                    var items = await tableClient.QueryAsync<ProfessionDataTableEntity>(x => x.ItemId == request.ItemId, cancellationToken: cancellationToken)
                        .ToArrayAsync(cancellationToken);
                    return items.Select(z => z.Player);
                }
                catch (Azure.RequestFailedException)
                {
                    throw new NotFoundException();
                }
            }
        }
    }
}