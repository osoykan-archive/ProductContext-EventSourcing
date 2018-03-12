using ProductContext.Common.Bus;

namespace ProductContext.Domain.Products
{
    public static class Events
    {
        public static class V1
        {
            public class ProductCreated : Message
            {
                public readonly int AgeGroupId;
                public readonly int BrandId;
                public readonly int BusinessUnitId;
                public readonly int GenderId;
                public readonly string ProductCode;
                public readonly string ProductId;

                public ProductCreated(string productId, string productCode, int brandId, int genderId, int ageGroupId, int businessUnitId)
                {
                    ProductId = productId;
                    ProductCode = productCode;
                    BrandId = brandId;
                    GenderId = genderId;
                    AgeGroupId = ageGroupId;
                    BusinessUnitId = businessUnitId;
                }
            }
        }
    }
}
