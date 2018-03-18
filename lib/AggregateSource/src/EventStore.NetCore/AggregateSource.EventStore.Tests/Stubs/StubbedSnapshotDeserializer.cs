using AggregateSource.EventStore.Snapshots;

using EventStore.ClientAPI;

namespace AggregateSource.EventStore.Tests.Stubs
{
    public class StubbedSnapshotDeserializer : ISnapshotDeserializer
    {
        public static readonly ISnapshotDeserializer Instance = new StubbedSnapshotDeserializer();

        private StubbedSnapshotDeserializer()
        {
        }

        public Snapshot Deserialize(ResolvedEvent resolvedEvent) => null;
    }
}
