using AggregateSource.EventStore.Stubs;

namespace AggregateSource.EventStore.Builders
{
    public class EventReaderConfigurationBuilder
    {
        public static readonly EventReaderConfigurationBuilder Default = new EventReaderConfigurationBuilder();
        private readonly IEventDeserializer _deserializer;
        private readonly SliceSize _sliceSize;
        private readonly IStreamNameResolver _streamNameResolver;
        private readonly IStreamUserCredentialsResolver _streamUserCredentialsResolver;

        private EventReaderConfigurationBuilder()
            : this(
                new SliceSize(1),
                StubbedEventDeserializer.Instance,
                StubbedStreamNameResolver.Instance,
                StubbedStreamUserCredentialsResolver.Instance)
        {
        }

        private EventReaderConfigurationBuilder(
            SliceSize sliceSize,
            IEventDeserializer deserializer,
            IStreamNameResolver streamNameResolver,
            IStreamUserCredentialsResolver streamUserCredentialsResolver)
        {
            _sliceSize = sliceSize;
            _deserializer = deserializer;
            _streamNameResolver = streamNameResolver;
            _streamUserCredentialsResolver = streamUserCredentialsResolver;
        }

        public EventReaderConfigurationBuilder UsingSliceSize(SliceSize value) => new EventReaderConfigurationBuilder(value, _deserializer, _streamNameResolver,
            _streamUserCredentialsResolver);

        public EventReaderConfigurationBuilder UsingDeserializer(IEventDeserializer value) => new EventReaderConfigurationBuilder(_sliceSize, value, _streamNameResolver,
            _streamUserCredentialsResolver);

        public EventReaderConfigurationBuilder UsingStreamNameResolver(IStreamNameResolver value) => new EventReaderConfigurationBuilder(_sliceSize, _deserializer, value, _streamUserCredentialsResolver);

        public EventReaderConfigurationBuilder UsingStreamUserCredentialsResolver(IStreamUserCredentialsResolver value) => new EventReaderConfigurationBuilder(_sliceSize, _deserializer, _streamNameResolver, value);

        public EventReaderConfiguration Build() => new EventReaderConfiguration(_sliceSize, _deserializer, _streamNameResolver,
            _streamUserCredentialsResolver);

        public static implicit operator EventReaderConfiguration(EventReaderConfigurationBuilder builder) => builder.Build();
    }
}
