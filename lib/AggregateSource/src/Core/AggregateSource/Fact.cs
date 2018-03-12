using System;

namespace AggregateSource
{
    /// <summary>
    ///     Represents the fact that an event happened to what is identified by the identifier.
    /// </summary>
    public struct Fact : IEquatable<Fact>
    {
        /// <summary>
        ///     Returns an empty array of facts.
        /// </summary>
        public static readonly Fact[] Empty = new Fact[0];

        /// <summary>
        ///     Initializes a new instance of the <see cref="Fact" /> struct.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="event">The event.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     Thrown if <paramref name="identifier" /> or <paramref name="event" /> is
        ///     <c>null</c>.
        /// </exception>
        public Fact(string identifier, object @event)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
        }

        /// <summary>
        ///     Gets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public string Identifier { get; }

        /// <summary>
        ///     Gets the event.
        /// </summary>
        /// <value>
        ///     The event.
        /// </value>
        public object Event { get; }

        /// <summary>
        ///     Determines whether the specified <see cref="Fact" /> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Fact" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="Fact" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Fact other) => Identifier.Equals(other.Identifier) &&
                                          Event.Equals(other.Event);

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Fact)obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => Identifier.GetHashCode() ^ Event.GetHashCode();

        /// <summary>
        ///     Implicitly converts a fact into a tuple.
        /// </summary>
        /// <param name="fact">The fact.</param>
        /// <returns>An tuple containing the fact data.</returns>
        public static implicit operator Tuple<string, object>(Fact fact) => new Tuple<string, object>(fact.Identifier, fact.Event);
    }
}
