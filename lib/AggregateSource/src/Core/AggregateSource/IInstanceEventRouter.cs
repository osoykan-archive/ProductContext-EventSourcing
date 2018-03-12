using System;
using System.Diagnostics.CodeAnalysis;

namespace AggregateSource
{
    /// <summary>
    ///     Routes an event to a configured state handler.
    /// </summary>
    public interface IInstanceEventRouter
    {
        /// <summary>
        ///     Routes the specified <paramref name="event" /> to a configured state handler, if any.
        /// </summary>
        /// <param name="event">The event to route.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="event" /> is null.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "event")]
        void Route(object @event);
    }
}
