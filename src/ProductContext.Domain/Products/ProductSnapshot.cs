using System.Collections.Generic;

namespace ProductContext.Domain.Products
{
    public class ProductSnapshot
    {
        public string ProductId { get; set; }

        public int BusinessUnitId { get; set; }

        public string Code { get; set; }

        public int BrandId { get; set; }

        public List<ProductContent> Contents { get; set; }

        public List<ProductVariant> Variants { get; set; }
    }
}
