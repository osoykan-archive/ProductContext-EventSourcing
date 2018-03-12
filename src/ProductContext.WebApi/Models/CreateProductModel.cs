namespace ProductContext.WebApi.Models
{
    public class CreateProductModel
    {
        public string Code { get; set; }

        public int BrandId { get; set; }

        public int AgeGroupId { get; set; }

        public int GenderId { get; set; }

        public int BusinessUnitId { get; set; }
    }
}
