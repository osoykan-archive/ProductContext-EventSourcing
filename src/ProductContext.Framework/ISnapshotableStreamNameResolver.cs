using System;

using AggregateSource.EventStore;

namespace ProductContext.Framework
{
    public class SnapshotableStreamNameResolver : IStreamNameResolver
    {
        private readonly Type _aggregateType;
        private readonly GetStreamName _getStreamName;

        public SnapshotableStreamNameResolver(Type aggregateType, GetStreamName getStreamName)
        {
            _aggregateType = aggregateType;
            _getStreamName = getStreamName;
        }

        public string Resolve(string identifier) => $"{_getStreamName(_aggregateType, identifier)}-Snapshot";
    }
}
