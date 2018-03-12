using System;
using System.Threading.Tasks;

using AggregateSource.EventStore;

using ProductContext.Common;
using ProductContext.Common.Bus;
using ProductContext.Domain.Aggregates;
using ProductContext.Domain.Commands;

namespace ProductContext.Domain.CommandHandlers
{
    public class ProductContentCommandHandlers : CommandHandlerBase<ProductContent>,
        IHandle<CreateProductContent>
    {
        public ProductContentCommandHandlers(AsyncRepository<ProductContent> repository, Func<DateTime> getDateTime)
            : base(repository, getDateTime)
        {
        }

        public Task HandleAsync(CreateProductContent command)
        {
            var productContentId = Guid.NewGuid().ToString();

            ProductContent productContent = ProductContent.Create(command.ProductId, productContentId, "Sari Pantolon");

            return Add(repository => { repository.Add(productContentId, productContent); });
        }
    }
}
