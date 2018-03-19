using System;

namespace AggregateSource.EventStore.Snapshots
{
    /// <summary>
    ///     Represents configuration settings used during reading from the snapshot store.
    /// </summary>
    public class SnapshotReaderConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SnapshotReaderConfiguration" /> class.
        /// </summary>
        /// <param name="deserializer">The snapshot deserializer.</param>
        /// <param name="streamNameResolver">The snapshot stream name resolver.</param>
        /// <param name="streamUserCredentialsResolver">The snapshot stream user credentials resolver.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     Thrown when <paramref name="deserializer" /> or
        ///     <paramref name="streamNameResolver" /> or <paramref name="streamUserCredentialsResolver" /> is <c>null</c>.
        /// </exception>
        public SnapshotReaderConfiguration(ISnapshotDeserializer deserializer, IStreamNameResolver streamNameResolver,
            IStreamUserCredentialsResolver streamUserCredentialsResolver)
        {
            Deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            StreamNameResolver = streamNameResolver ?? throw new ArgumentNullException(nameof(streamNameResolver));
            StreamUserCredentialsResolver = streamUserCredentialsResolver ?? throw new ArgumentNullException(nameof(streamUserCredentialsResolver));
        }

        /// <summary>
        ///     Gets the snapshot deserializer.
        /// </summary>
        /// <value>
        ///     The deserializer.
        /// </value>
        public ISnapshotDeserializer Deserializer { get; }

        /// <summary>
        ///     Gets the snapshot stream name resolver.
        /// </summary>
        /// <value>
        ///     The resolver.
        /// </value>
        public IStreamNameResolver StreamNameResolver { get; }

        /// <summary>
        ///     Gets the snapshot stream user credentials resolver.
        /// </summary>
        /// <value>
        ///     The resolver.
        /// </value>
        public IStreamUserCredentialsResolver StreamUserCredentialsResolver { get; }
    }
}
