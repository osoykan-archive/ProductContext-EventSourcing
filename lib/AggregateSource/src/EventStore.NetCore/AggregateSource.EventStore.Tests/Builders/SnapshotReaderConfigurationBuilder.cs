using AggregateSource.EventStore.NetCore;
using AggregateSource.EventStore.NetCore.Snapshots;
using AggregateSource.EventStore.Stubs;

namespace AggregateSource.EventStore.Builders
{
    public class SnapshotReaderConfigurationBuilder
    {
        public static readonly SnapshotReaderConfigurationBuilder Default = new SnapshotReaderConfigurationBuilder();
        private readonly ISnapshotDeserializer _deserializer;
        private readonly IStreamNameResolver _streamNameResolver;
        private readonly IStreamUserCredentialsResolver _streamUserCredentialsResolver;

        private SnapshotReaderConfigurationBuilder()
            : this(
                StubbedSnapshotDeserializer.Instance,
                StubbedStreamNameResolver.Instance,
                StubbedStreamUserCredentialsResolver.Instance)
        {
        }

        private SnapshotReaderConfigurationBuilder(
            ISnapshotDeserializer deserializer,
            IStreamNameResolver streamNameResolver,
            IStreamUserCredentialsResolver streamUserCredentialsResolver)
        {
            _deserializer = deserializer;
            _streamNameResolver = streamNameResolver;
            _streamUserCredentialsResolver = streamUserCredentialsResolver;
        }

        public SnapshotReaderConfigurationBuilder UsingDeserializer(ISnapshotDeserializer value) => new SnapshotReaderConfigurationBuilder(value, _streamNameResolver, _streamUserCredentialsResolver);

        public SnapshotReaderConfigurationBuilder UsingStreamNameResolver(IStreamNameResolver value) => new SnapshotReaderConfigurationBuilder(_deserializer, value, _streamUserCredentialsResolver);

        public SnapshotReaderConfigurationBuilder UsingStreamUserCredentialsResolver(
            IStreamUserCredentialsResolver value) => new SnapshotReaderConfigurationBuilder(_deserializer, _streamNameResolver, value);

        public SnapshotReaderConfiguration Build() => new SnapshotReaderConfiguration(_deserializer, _streamNameResolver, _streamUserCredentialsResolver);

        public static implicit operator SnapshotReaderConfiguration(SnapshotReaderConfigurationBuilder builder) => builder.Build();
    }
}
