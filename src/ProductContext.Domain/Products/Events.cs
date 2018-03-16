using ProductContext.Framework;

namespace ProductContext.Domain.Products
{
    public static class Events
    {
        public static class V1
        {
            public class ProductCreated : Message
            {
                public readonly int BrandId;
                public readonly int BusinessUnitId;
                public readonly string ProductCode;
                public readonly string ProductId;

                public ProductCreated(string productId, string productCode, int brandId, int businessUnitId)
                {
                    ProductId = productId;
                    ProductCode = productCode;
                    BrandId = brandId;
                    BusinessUnitId = businessUnitId;
                }
            }

            public class VariantAddedToProduct : Message
            {
                public readonly string Barcode;
                public readonly string ProductContentId;
                public readonly string ProductId;
                public readonly string ProductVariantId;
                public readonly string VariantTypeValueId;
                public readonly int VariantType;

                public VariantAddedToProduct(
                    string productId,
                    string productContentId,
                    string productVariantId,
                    string barcode,
                    string variantTypeValueId,
                    int variantType)
                {
                    ProductId = productId;
                    ProductContentId = productContentId;
                    Barcode = barcode;
                    ProductVariantId = productVariantId;
                    VariantTypeValueId = variantTypeValueId;
                    VariantType = variantType;
                }
            }

            public class ContentAddedToProduct : Message
            {
                public readonly string Description;
                public readonly string ProductContentId;
                public readonly string ProductId;
                public readonly string VariantTypeValueId;
                public readonly int ProductContentStatus;
                public readonly int VariantType;

                public ContentAddedToProduct(
                    string productId,
                    string productContentId,
                    string description,
                    string variantTypeValueId,
                    int productContentStatus,
                    int variantType)
                {
                    ProductId = productId;
                    Description = description;
                    VariantTypeValueId = variantTypeValueId;
                    ProductContentStatus = productContentStatus;
                    VariantType = variantType;
                    ProductContentId = productContentId;
                }
            }

            public class VariantTypeValueCreated : Message
            {
                public readonly string Code;
                public readonly string Value;
                public readonly int VariantType;

                public VariantTypeValueCreated(string value, string code, int variantType)
                {
                    Value = value;
                    Code = code;
                    VariantType = variantType;
                }
            }
        }
    }
}
