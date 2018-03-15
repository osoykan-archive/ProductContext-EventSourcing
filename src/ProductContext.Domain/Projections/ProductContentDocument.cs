namespace ProductContext.Domain.Projections
{
    public class ProductContentDocument
    {
        public string ProductContentId { get; set; }

        public string VariantTypeValueId { get; set; }

        public int VariantTypeId { get; set; }

        public string Description { get; set; }

        public int Status { get; set; }
    }
}
