using DapperExtensions.Mapper;

namespace ProductContext.Domain.Projections
{
    public class ProductProjection
    {
        public string ProductId { get; set; }

        public string Code { get; set; }

        public int BrandId { get; set; }

        public int GenderId { get; set; }

        public int AgeGroupId { get; set; }

        public int BusinessUnitId { get; set; }
    }

    public class ProductProjectionMap : AutoClassMapper<ProductProjection>
    {
        public ProductProjectionMap()
        {
            Table("Product");
            Schema("dbo");
        }
    }
}
