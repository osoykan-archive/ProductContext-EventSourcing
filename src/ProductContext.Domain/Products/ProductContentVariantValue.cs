using System.Collections.Generic;

using ProductContext.Domain.Contracts;

using Value;

namespace ProductContext.Domain.Products
{
    public class ProductContentVariantValue : ValueType<ProductContentVariantValue>
    {
        public ProductContentVariantValue(ProductId productId, VariantTypeValueId variantTypeValueId, Enums.VariantType variantType)
        {
            ProductId = productId;
            VariantTypeValueId = variantTypeValueId;
            VariantType = variantType;
        }

        public ProductId ProductId { get; }

        public VariantTypeValueId VariantTypeValueId { get; }

        public Enums.VariantType VariantType { get; }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield break;
        }
    }
}
