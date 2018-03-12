using System;
using System.Collections.Generic;

using AggregateSource;

using ProductContext.Domain.Entities;
using ProductContext.Domain.Events;

namespace ProductContext.Domain.Aggregates
{
    public partial class ProductContentVariant : AggregateRootEntity
    {
        public static readonly Func<ProductContentVariant> Factory = () => new ProductContentVariant();

        private ProductContentVariant()
        {
            Register<ProductContentVariantCreated>(When);
            Register<ListingAddedToProductContentVariant>(When);
        }

        public ProductContentId ProductContentId { get; private set; }

        public ProductContentVariantId ProductContentVariantId { get; private set; }

        public List<ProductContentVariantListing> Listings { get; private set; }

        public ProductId ProductId { get; private set; }

        public int ItemNumber { get; private set; }

        public string Barcode { get; private set; }

        public static ProductContentVariant Create(string productContentVariantId, string productId, string productContentId, int itemNumber, string barcode)
        {
            ProductContentVariant aggregate = Factory();

            aggregate.ApplyChange(
                new ProductContentVariantCreated(productId, productContentId, productContentVariantId, barcode, itemNumber)
                );

            return aggregate;
        }

        public void AddListing(string listingId)
        {
            ApplyChange(
                new ListingAddedToProductContentVariant(ProductId.Id, ProductContentId.Id, ProductContentVariantId.Id, listingId)
                );
        }
    }
}
