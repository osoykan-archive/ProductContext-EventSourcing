namespace ProductContext.Domain.Products.Snapshots
{
    public class ProductContentSnapshot
    {
        public string ProductId { get; set; }

        public string ProductContentId { get; set; }

        public ProductContentVariantValueSnapshot VariantValue { get; set; }

        public string Description { get; set; }

        public int Status { get; set; }
    }
}
