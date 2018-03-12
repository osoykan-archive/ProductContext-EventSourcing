using ProductContext.Common.Bus;

namespace ProductContext.Domain.Commands
{
    public class CreateProduct : Message
    {
        public CreateProduct(string code, int brandId, int ageGroupId, int genderId, int businessUnitId)
        {
            Code = code;
            BrandId = brandId;
            AgeGroupId = ageGroupId;
            GenderId = genderId;
            BusinessUnitId = businessUnitId;
        }

        public string Code { get; }

        public int BrandId { get; }

        public int AgeGroupId { get; }

        public int GenderId { get; }

        public int BusinessUnitId { get; }
    }
}
