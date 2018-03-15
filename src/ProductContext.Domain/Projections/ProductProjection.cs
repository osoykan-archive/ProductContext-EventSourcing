using Couchbase;
using Couchbase.Core;

using ProductContext.Domain.Products;
using ProductContext.Framework;

using Projac;

namespace ProductContext.Domain.Projections
{
    public class ProductProjection : Projection<IBucket>
    {
        public ProductProjection()
        {
            When<Envelope<Events.V1.ProductCreated>>((bucket, e) =>
            {
                bucket.InsertAsync(new Document<ProductDocument>
                {
                    Id = e.Message.ProductId,
                    Content = new ProductDocument
                    {
                        ProductId = e.Message.ProductId,
                        AgeGroupId = e.Message.AgeGroupId,
                        BrandId = e.Message.BrandId,
                        BusinessUnitId = e.Message.BrandId,
                        Code = e.Message.ProductCode,
                        GenderId = e.Message.GenderId
                    }
                });
            });

            When<SetProjectionPosition>(async (bucket, e) =>
            {
                string id = CouchbaseCheckpointStore.GetCheckpointDocumentId(nameof(ProductProjection));
                IOperationResult<CheckpointDocument> checkpoint = await bucket.GetAsync<CheckpointDocument>(id);

                if (checkpoint.Value != null)
                {
                    checkpoint.Value.Checkpoint = e.Position;
                    await bucket.ReplaceAsync(new Document<CheckpointDocument>
                    {
                        Content = checkpoint.Value,
                        Id = checkpoint.Id
                    });
                }
                else
                {
                    await bucket.InsertAsync(new Document<CheckpointDocument>
                    {
                        Id = id,
                        Content = new CheckpointDocument { Checkpoint = e.Position }
                    });
                }
            });
        }
    }
}
