using System;

using AggregateSource;

using ProductContext.Domain.Contracts;

namespace ProductContext.Domain.Products
{
    public class VariantTypeValue : AggregateRootEntity
    {
        public static readonly Func<VariantTypeValue> Factory = () => new VariantTypeValue();

        public Enums.VariantType VariantType { get; private set; }

        public string Value { get; private set; }

        public string Code { get; private set; }

        public static VariantTypeValue Create(string value, string code, Enums.VariantType variantType)
        {
            VariantTypeValue aggregate = Factory();

            aggregate.ApplyChange(
                new Events.V1.VariantTypeValueCreated(value, code, (int)variantType)
            );

            return aggregate;
        }
    }
}
