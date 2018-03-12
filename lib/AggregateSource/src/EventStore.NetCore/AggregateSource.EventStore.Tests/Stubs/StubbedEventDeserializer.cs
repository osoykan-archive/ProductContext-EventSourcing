using System.Collections.Generic;
using System.Linq;

using AggregateSource.EventStore.NetCore;

using EventStore.ClientAPI;

namespace AggregateSource.EventStore.Stubs
{
    public class StubbedEventDeserializer : IEventDeserializer
    {
        public static readonly IEventDeserializer Instance = new StubbedEventDeserializer();

        private StubbedEventDeserializer()
        {
        }

        public IEnumerable<object> Deserialize(ResolvedEvent resolvedEvent) => Enumerable.Empty<object>();
    }
}
