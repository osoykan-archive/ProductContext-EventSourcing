using System;
using System.Text;

using AggregateSource.EventStore;

using EventStore.ClientAPI;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ProductContext.Framework
{
    public class DefaultEventDeserializer : IEventDeserializer
    {
        public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore
        };

        public object Deserialize(ResolvedEvent resolvedEvent) =>
            JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                Type.GetType(resolvedEvent.Event.EventType, true),
                DefaultSettings
            );
    }
}
