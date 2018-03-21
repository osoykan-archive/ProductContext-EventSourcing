using System;
using System.Threading.Tasks;

using AggregateSource.EventStore;
using AggregateSource.EventStore.Snapshots;

using ProductContext.Domain.Contracts;
using ProductContext.Framework;

namespace ProductContext.Domain.Products
{
    public class ProductCommandHandlers : CommandHandlerBase<Product>,
        IHandle<Commands.V1.CreateProduct>,
        IHandle<Commands.V1.AddContentToProduct>,
        IHandle<Commands.V1.AddVariantToProduct>

    {
        public ProductCommandHandlers(
            GetStreamName getStreamName,
            AsyncRepository<Product> repository,
            AsyncSnapshotableRepository<Product> snapshotableRepository,
            Now now) : base(getStreamName, repository, snapshotableRepository, now)
        {
        }

        public Task HandleAsync(Commands.V1.AddContentToProduct message) =>
            Update(message.ProductId, async product =>
            {
                product.AddContent(message.ProductContentId, message.Description, message.VariantTypeValueId);
            });

        public Task HandleAsync(Commands.V1.AddVariantToProduct message) =>
            Update(message.ProductId, async product =>
            {
                product.AddVariant(message.ContentId, message.VariantId, message.Barcode, message.VariantTypeValueId);
            });

        public Task HandleAsync(Commands.V1.CreateProduct command) =>
            Add(async repository =>
            {
                Product product = Product.Create(
                    command.ProductId,
                    command.BrandId,
                    command.Code,
                    command.BusinessUnitId);

                repository.Add(command.ProductId, product);
            });
    }
}
