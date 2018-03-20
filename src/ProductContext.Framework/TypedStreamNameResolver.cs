using System;

using AggregateSource.EventStore;

namespace ProductContext.Framework
{
    public class TypedStreamNameResolver : IStreamNameResolver
    {
        private readonly GetStreamName _getStreamName;
        private readonly Type _streamType;

        public TypedStreamNameResolver(Type streamType, GetStreamName getStreamName)
        {
            _streamType = streamType;
            _getStreamName = getStreamName;
        }

        public string Resolve(string identifier) => $"{_getStreamName(_streamType, identifier)}";
    }
}
