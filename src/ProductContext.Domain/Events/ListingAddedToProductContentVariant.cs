using ProductContext.Common.Bus;

namespace ProductContext.Domain.Events
{
    public class ListingAddedToProductContentVariant : Message
    {
        public readonly string ListingId;
        public readonly string ProductContentId;
        public readonly string ProductId;
        public readonly string ProductContentVariantId;

        public ListingAddedToProductContentVariant(string productId, string productContentId, string productContentVariantId, string listingId)
        {
            ProductId = productId;
            ProductContentId = productContentId;
            ProductContentVariantId = productContentVariantId;
            ListingId = listingId;
        }
    }
}
