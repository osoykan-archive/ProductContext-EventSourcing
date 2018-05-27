using System.Threading;
using System.Threading.Tasks;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Snapshots;
using MediatR;
using ProductContext.Domain.Contracts;
using ProductContext.Framework;

namespace ProductContext.Domain.Products
{
    public class ProductCommandHandlers : CommandHandlerBase<Product>,
        IRequestHandler<Commands.V1.CreateProduct>,
        IRequestHandler<Commands.V1.AddContentToProduct>,
        IRequestHandler<Commands.V1.AddVariantToProduct>

    {
        public ProductCommandHandlers(
            GetStreamName getStreamName,
            AsyncRepository<Product> repository,
            AsyncSnapshotableRepository<Product> snapshotableRepository,
            Now now) : base(getStreamName, repository, snapshotableRepository, now)
        {
        }

        public Task Handle(Commands.V1.AddContentToProduct message, CancellationToken cancellationToken) =>
            Update(message.ProductId,
                async product =>
                {
                    product.AddContent(message.ProductContentId, message.Description, message.VariantTypeValueId);
                });

        public Task Handle(Commands.V1.AddVariantToProduct message, CancellationToken cancellationToken) =>
            Update(message.ProductId,
                async product =>
                {
                    product.AddVariant(message.ContentId, message.VariantId, message.Barcode,
                        message.VariantTypeValueId);
                });

        public Task Handle(Commands.V1.CreateProduct command, CancellationToken cancellationToken) =>
            Add(async repository =>
            {
                var product = Product.Create(
                    command.ProductId,
                    command.BrandId,
                    command.Code,
                    command.BusinessUnitId);

                repository.Add(command.ProductId, product);
            });
    }
}