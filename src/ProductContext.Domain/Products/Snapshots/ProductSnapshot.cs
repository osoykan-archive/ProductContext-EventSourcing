using System.Collections.Generic;

namespace ProductContext.Domain.Products.Snapshots
{
    public class ProductSnapshot
    {
        public string ProductId { get; set; }

        public int BusinessUnitId { get; set; }

        public string Code { get; set; }

        public int BrandId { get; set; }

        public List<ProductContentSnapshot> Contents { get; set; }

        public List<ProductVariantSnapshot> Variants { get; set; }
    }
}
