namespace AggregateSource
{
    /// <summary>
    ///     Mutable class that builds up an <see cref="Aggregate" />.
    /// </summary>
    public class AggregateBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AggregateBuilder" /> class.
        /// </summary>
        public AggregateBuilder()
        {
            Identifier = null;
            ExpectedVersion = int.MinValue;
            Root = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AggregateBuilder" /> class.
        /// </summary>
        /// <param name="instance">The aggregate instance to copy data from.</param>
        internal AggregateBuilder(Aggregate instance)
        {
            Identifier = instance.Identifier;
            ExpectedVersion = instance.ExpectedVersion;
            Root = instance.Root;
        }

        /// <summary>
        ///     Gets the aggregate identifier.
        /// </summary>
        /// <value>
        ///     The aggregate identifier.
        /// </value>
        public string Identifier { get; private set; }

        /// <summary>
        ///     Gets the aggregate version.
        /// </summary>
        public int ExpectedVersion { get; private set; }

        /// <summary>
        ///     Gets the aggregate root entity.
        /// </summary>
        /// <value>
        ///     The aggregate root entity.
        /// </value>
        public IAggregateRootEntity Root { get; private set; }

        /// <summary>
        ///     Captures the identity of the aggregate.
        /// </summary>
        /// <param name="value">The identifier value.</param>
        /// <returns>An <see cref="AggregateBuilder" /> instance.</returns>
        public AggregateBuilder IdentifiedBy(string value)
        {
            Identifier = value;
            return this;
        }

        /// <summary>
        ///     Captures the expected version of the aggregate.
        /// </summary>
        /// <param name="value">The expected version value</param>
        /// <returns>An <see cref="AggregateBuilder" /> instance.</returns>
        public AggregateBuilder ExpectVersion(int value)
        {
            ExpectedVersion = value;
            return this;
        }

        /// <summary>
        ///     Captures the aggregate root entity of the aggregate.
        /// </summary>
        /// <param name="value">The aggregate root entity value.</param>
        /// <returns>An <see cref="AggregateBuilder" /> instance.</returns>
        public AggregateBuilder WithRoot(IAggregateRootEntity value)
        {
            Root = value;
            return this;
        }

        /// <summary>
        ///     Builds the immutable <see cref="Aggregate" /> based on captured information.
        /// </summary>
        /// <returns>An <see cref="Aggregate" />.</returns>
        public Aggregate Build() => new Aggregate(Identifier, ExpectedVersion, Root);
    }
}
