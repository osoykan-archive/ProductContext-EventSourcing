using System;

using AggregateSource.EventStore;

namespace ProductContext.Framework
{
    public class SnapshotableStreamNameResolver : IStreamNameResolver
    {
        private readonly Type _aggregateType;
        private readonly GetSnapshotStreamName _getSnapshotStreamName;

        public SnapshotableStreamNameResolver(Type aggregateType, GetSnapshotStreamName getSnapshotStreamName)
        {
            _aggregateType = aggregateType;
            _getSnapshotStreamName = getSnapshotStreamName;
        }

        public string Resolve(string identifier) => $"{_getSnapshotStreamName(_aggregateType, identifier)}";
    }
}
