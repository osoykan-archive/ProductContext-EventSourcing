namespace ProductContext.Domain.Projections
{
    public class ProductVariantDocument
    {
        public string ProductId { get; set; }

        public string ProductContentId { get; set; }

        public string Barcode { get; set; }

        public string ProductVariantId { get; set; }

        public string VariantTypeValueId { get;set; }

        public int VariantType { get; set; }
    }
}
