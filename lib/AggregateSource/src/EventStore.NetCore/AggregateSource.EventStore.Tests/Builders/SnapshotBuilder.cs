using AggregateSource.EventStore.NetCore.Snapshots;

namespace AggregateSource.EventStore.Builders
{
    public class SnapshotBuilder
    {
        public static readonly SnapshotBuilder Default = new SnapshotBuilder();

        private SnapshotBuilder() : this(0, new object())
        {
        }

        private SnapshotBuilder(int version, object state)
        {
            State = state;
            Version = version;
        }

        public object State { get; }

        public int Version { get; }

        public SnapshotBuilder WithState(object value) => new SnapshotBuilder(Version, value);

        public SnapshotBuilder WithVersion(int value) => new SnapshotBuilder(value, State);

        public Snapshot Build() => new Snapshot(Version, State);

        public static implicit operator Snapshot(SnapshotBuilder builder) => builder.Build();
    }
}
