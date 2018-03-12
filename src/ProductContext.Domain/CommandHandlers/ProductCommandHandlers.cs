using System;
using System.Threading.Tasks;

using AggregateSource.EventStore;

using ProductContext.Common;
using ProductContext.Common.Bus;
using ProductContext.Domain.Aggregates;
using ProductContext.Domain.Commands;

namespace ProductContext.Domain.CommandHandlers
{
    public class ProductCommandHandlers : CommandHandlerBase<Product>,
        IHandle<CreateProduct>

    {
        public ProductCommandHandlers(AsyncRepository<Product> repository, Func<DateTime> getDateTime)
            : base(repository, getDateTime)
        {
        }

        public Task HandleAsync(CreateProduct command)
        {
            string productId = Guid.NewGuid().ToString();

            Product product = Product.Create(
                productId,
                command.BrandId,
                command.Code,
                command.GenderId,
                command.AgeGroupId,
                command.BusinessUnitId);

            return Add(repository => { repository.Add(productId, product); });
        }
    }
}
