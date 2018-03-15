namespace ProductContext.Domain.Projections
{
    public class ProductDocument
    {
        public string ProductId { get; set; }

        public string Code { get; set; }

        public int BrandId { get; set; }

        public int GenderId { get; set; }

        public int AgeGroupId { get; set; }

        public int BusinessUnitId { get; set; }
    }
}
