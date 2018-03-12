using ProductContext.Common.Bus;
using ProductContext.Domain.Enums;

namespace ProductContext.Domain.Events
{
    public class ProductContentCreated : Message
    {
        public readonly string ProductId;
        public readonly string ProductContentId;
        public readonly string Description;
        public readonly int ProductContentStatus;
        public readonly bool IsOnBlackList;

        public ProductContentCreated(string productId, string productContentId, string description, int productContentStatus, bool isOnBlackList)
        {
            ProductId = productId;
            Description = description;
            ProductContentStatus = productContentStatus;
            IsOnBlackList = isOnBlackList;
            ProductContentId = productContentId;
        }
    }
}
