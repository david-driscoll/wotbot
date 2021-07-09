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
    public static class UploadProfessionData
    {
        public record Request(string Player, string Profession, IEnumerable<Item> Items) : IRequest;
        public record Item(int Id, string Name);

        class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(z => z.Player).NotNull();
                RuleFor(z => z.Profession).NotNull();
                RuleFor(z => z.Items).NotNull();
            }
        }

        class Handler : IRequestHandler<Request>
        {
            private readonly ITableClientFactory _tableClientFactory;

            public Handler(ITableClientFactory tableClientFactory, IMapper mapper)
            {
                _tableClientFactory = tableClientFactory;
            }

            public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var tableClient = _tableClientFactory.CreateClient(Constants.ProfessionsTable);
                await tableClient.CreateIfNotExistsAsync(cancellationToken);

                var items = request.Items
                    .Select(item => new ProfessionDataTableEntity()
                    {
                        Player = request.Player,
                        Profession = request.Profession,
                        ItemId = item.Id,
                        ItemName = item.Name
                    })
                    .Select(profile => new TableTransactionAction(TableTransactionActionType.UpsertReplace, profile))
                    .ToArray();

                if (items.Any())
                {
                    foreach (var buffer in items.Buffer(100))
                    {
                        await tableClient.SubmitTransactionAsync(buffer, cancellationToken);
                    }
                }
                return Unit.Value;
            }
        }
    }
}
