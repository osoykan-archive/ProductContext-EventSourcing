using ProductContext.Common.Bus;

namespace ProductContext.Domain.Events
{
    public class ProductContentVariantCreated : Message
    {
        public readonly string Barcode;
        public readonly int ItemNumber;
        public readonly string ProductContentId;
        public readonly string ProductId;
        public readonly string ProductContentVariantId;

        public ProductContentVariantCreated(
            string productId,
            string productContentId,
            string productContentVariantId,
            string barcode,
            int itemNumber)
        {
            ProductId = productId;
            ProductContentId = productContentId;
            ProductContentVariantId = productContentVariantId;
            Barcode = barcode;
            ItemNumber = itemNumber;
        }
    }
}
