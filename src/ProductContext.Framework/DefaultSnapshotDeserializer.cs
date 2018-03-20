using System;
using System.Text;

using AggregateSource.EventStore.Snapshots;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace ProductContext.Framework
{
    public class DefaultSnapshotDeserializer : ISnapshotDeserializer
    {
        public Snapshot Deserialize(ResolvedEvent resolvedEvent)
        {
            object obj = JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                Type.GetType(resolvedEvent.Event.EventType, true),
                DefaultEventDeserializer.DefaultSettings
                );

            var metadata = JsonConvert.DeserializeObject<EventMetadata>(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata));
            return new Snapshot(metadata.Version, obj);
        }
    }
}
