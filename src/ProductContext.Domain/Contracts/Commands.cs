using ProductContext.Framework;

namespace ProductContext.Domain.Contracts
{
    public static class Commands
    {
        public static class V1
        {
            public class CreateProduct : Message
            {
                public string Code { get; set; }

                public int BrandId { get; set; }

                public int AgeGroupId { get; set; }

                public int GenderId { get; set; }

                public int BusinessUnitId { get; set; }
            }
        }
    }
}
