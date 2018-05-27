using MediatR;
using ProductContext.Framework;

namespace ProductContext.Domain.Contracts
{
    public static class Commands
    {
        public static class V1
        {
            public class CreateProduct : IRequest
            {
                public string Code { get; set; }

                public int BrandId { get; set; }

                public string ProductId { get; set; }

                public int BusinessUnitId { get; set; }
            }

            public class AddContentToProduct : IRequest
            {
                public string ProductId { get; set; }

                public string ProductContentId { get; set; }

                public string Description { get; set; }

                public string VariantTypeValueId { get; set; }
            }

            public class AddVariantToProduct : IRequest
            {
                public string ProductId { get; set; }

                public string VariantId { get; set; }

                public string Barcode { get; set; }

                public string VariantTypeValueId { get; set; }

                public string ContentId { get; set; }
            }
        }
    }
}
