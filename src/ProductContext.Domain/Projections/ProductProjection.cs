using System.Linq;

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
                        BrandId = e.Message.BrandId,
                        BusinessUnitId = e.Message.BrandId,
                        Code = e.Message.ProductCode,
                        Position = e.Position
                    }
                });
            });

            When<Envelope<Events.V1.ContentAddedToProduct>>(async (bucket, e) =>
            {
                IOperationResult<ProductDocument> productDocument = await bucket.GetAsync<ProductDocument>(e.Message.ProductId);
                ProductDocument product = productDocument.Value;
                if (product != null && product.Position < e.Position)
                {
                    if (product.Contents.Any(x => x.ProductContentId == e.Message.ProductContentId))
                    {
                        return;
                    }

                    product.Contents.Add(new ProductContentDocument
                    {
                        ProductContentId = e.Message.ProductContentId,
                        VariantTypeValueId = e.Message.VariantTypeValueId,
                        Status = e.Message.ProductContentStatus,
                        Description = e.Message.Description,
                        VariantTypeId = e.Message.VariantType
                    });

                    await bucket.UpsertAsync(new Document<ProductDocument>()
                    {
                        Id = e.Message.ProductId,
                        Content = product
                    });
                }
            });

            When<Envelope<Events.V1.VariantAddedToProduct>>(async (bucket, e) =>
            {
                IOperationResult<ProductDocument> productDocument = await bucket.GetAsync<ProductDocument>(e.Message.ProductId);
                ProductDocument product = productDocument.Value;
                if (product != null && product.Position < e.Position)
                {
                    if (product.Variants.Any(x => x.ProductVariantId == e.Message.ProductVariantId))
                    {
                        return;
                    }

                    product.Variants.Add(new ProductVariantDocument()
                    {
                        ProductId = e.Message.ProductId,
                        Barcode = e.Message.Barcode,
                        ProductContentId = e.Message.ProductContentId,
                        ProductVariantId = e.Message.ProductVariantId,
                        VariantTypeValueId = e.Message.VariantTypeValueId,
                        VariantType = e.Message.VariantType
                    });

                    await bucket.UpsertAsync(new Document<ProductDocument>()
                    {
                        Id = e.Message.ProductId,
                        Content = product
                    });
                }

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
