namespace ProductContext.Domain.Products.Snapshots
{
    public class ProductVariantSnapshot
    {
        public string ProductContentId { get; set; }

        public string ProductVariantId { get; set; }

        public string ProductId { get; set; }

        public ProductVariantTypeValueSnapshot VariantValue { get; set; }

        public string Barcode { get; set; }
    }
}
