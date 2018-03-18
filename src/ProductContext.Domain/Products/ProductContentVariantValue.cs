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

        public ProductId ProductId { get; private set; }

        public VariantTypeValueId VariantTypeValueId { get; private set; }

        public Enums.VariantType VariantType { get; private set; }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object> { ProductId, VariantTypeValueId, VariantType };
        }
    }
}
