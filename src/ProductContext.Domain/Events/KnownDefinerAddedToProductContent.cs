using ProductContext.Common.Bus;

namespace ProductContext.Domain.Events
{
    public class KnownDefinerAddedToProductContent : Message
    {
        public readonly string ProductContentId;
        public readonly string ProductContentKnownDefinerId;
        public readonly int ProductContentKnownDefinerType;
        public readonly string Value;
        public readonly string ValueHash;

        public KnownDefinerAddedToProductContent(string productContentKnownDefinerId, string productContentId, int productContentKnownDefinerType, string value, string valueHash)
        {
            ProductContentKnownDefinerId = productContentKnownDefinerId;
            ProductContentId = productContentId;
            ProductContentKnownDefinerType = productContentKnownDefinerType;
            Value = value;
            ValueHash = valueHash;
        }
    }
}
