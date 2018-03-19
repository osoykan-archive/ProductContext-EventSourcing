using System;
using System.Text;

using AggregateSource.EventStore.Snapshots;

using EventStore.ClientAPI;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ProductContext.Framework
{
    public class DefaultSnapshotDeserializer : ISnapshotDeserializer
    {
        public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore
        };

        public Snapshot Deserialize(ResolvedEvent resolvedEvent)
        {
            object obj = JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                Type.GetType(resolvedEvent.Event.EventType, true),
                DefaultSettings
                );

            return new Snapshot(BitConverter.ToInt32(resolvedEvent.Event.Metadata, 0), obj);
        }
    }
}
