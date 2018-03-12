using System;

namespace AggregateSource
{
    /// <summary>
    ///     Class for tracking aggregate meta data and its <see cref="IAggregateRootEntity" />.
    /// </summary>
    public class Aggregate
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Aggregate" /> class.
        /// </summary>
        /// <param name="identifier">The aggregate identifier.</param>
        /// <param name="expectedVersion">The expected aggregate version.</param>
        /// <param name="root">The aggregate root entity.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="identifier" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="root" /> is null.</exception>
        public Aggregate(string identifier, int expectedVersion, IAggregateRootEntity root)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            ExpectedVersion = expectedVersion;
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>
        ///     Gets the aggregate identifier.
        /// </summary>
        /// <value>
        ///     The aggregate identifier.
        /// </value>
        public string Identifier { get; }

        /// <summary>
        ///     Gets the aggregate version.
        /// </summary>
        public int ExpectedVersion { get; }

        /// <summary>
        ///     Gets the aggregate root entity.
        /// </summary>
        /// <value>
        ///     The aggregate root entity.
        /// </value>
        public IAggregateRootEntity Root { get; }

        /// <summary>
        ///     Creates a mutable builder with the same contents as this instance.
        /// </summary>
        /// <returns>An <see cref="AggregateBuilder" />.</returns>
        public AggregateBuilder ToBuilder() => new AggregateBuilder(this);
    }
}
