using System;
using System.Collections.Generic;
using System.Text;

using AggregateSource.EventStore;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace ProductContext.Common
{
    public class DefaultEventDeserializer : IEventDeserializer
    {
        public IEnumerable<object> Deserialize(ResolvedEvent resolvedEvent) => new List<object>
        {
            JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                Type.GetType(resolvedEvent.Event.EventType, true)
                )
        };
    }
}
