using System.Collections.Generic;

using ProductContext.Domain.Entities;
using ProductContext.Domain.Events;

namespace ProductContext.Domain.Aggregates
{
    public partial class ProductContentVariant
    {
        private void When(ProductContentVariantCreated @event)
        {
            ProductId = new ProductId(@event.ProductId);
            ProductContentId = new ProductContentId(@event.ProductContentId);
            ProductContentVariantId = new ProductContentVariantId(@event.ProductContentVariantId);
            Barcode = @event.Barcode;
            ItemNumber = @event.ItemNumber;
            Listings = new List<ProductContentVariantListing>();
        }

        private void When(ListingAddedToProductContentVariant @event)
        {
            var listing = new ProductContentVariantListing(ApplyChange);
            listing.Route(@event);
            Listings.Add(listing);
        }
    }
}
