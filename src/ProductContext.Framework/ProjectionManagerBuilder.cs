using System;
using System.Threading.Tasks;

using AggregateSource.EventStore;

using EventStore.ClientAPI;

using Projac;

namespace ProductContext.Framework
{
    public class ProjectionManagerBuilder
    {
        public static readonly ProjectionManagerBuilder With = new ProjectionManagerBuilder();
        private ICheckpointStore _checkpointStore;
        private IEventStoreConnection _connection;
        private IEventDeserializer _deserializer;
        private int? _maxLiveQueueSize;
        private ProjectorDefiner[] _projections;
        private int? _readBatchSize;
        private ISnapshotter[] _snapshotters;

        public ProjectionManagerBuilder Connection(IEventStoreConnection connection)
        {
            _connection = connection;
            return this;
        }

        public ProjectionManagerBuilder Deserializer(IEventDeserializer deserializer)
        {
            _deserializer = deserializer;
            return this;
        }

        public ProjectionManagerBuilder MaxLiveQueueSize(int maxLiveQueueSize)
        {
            _maxLiveQueueSize = maxLiveQueueSize;
            return this;
        }

        public ProjectionManagerBuilder CheckpointStore(ICheckpointStore checkpointStore)
        {
            _checkpointStore = checkpointStore;
            return this;
        }

        public ProjectionManagerBuilder ReadBatchSize(int readBatchSize)
        {
            _readBatchSize = readBatchSize;
            return this;
        }

        public ProjectionManagerBuilder Snaphotter(params ISnapshotter[] snapshotters)
        {
            _snapshotters = snapshotters;
            return this;
        }

        public ProjectionManager<TConnection> Build<TConnection>(Func<TConnection> connection) =>
            new ProjectionManager<TConnection>(_connection, _deserializer, connection, _checkpointStore, _projections, _snapshotters, _maxLiveQueueSize, _readBatchSize);

        public async Task<ProjectionManager<TConnection>> Activate<TConnection>(Func<TConnection> getConnection)
        {
            ProjectionManager<TConnection> manager = Build(getConnection);
            await manager.Activate();
            return manager;
        }

        public ProjectionManagerBuilder Projections(params ProjectorDefiner[] projections)
        {
            _projections = projections;
            return this;
        }
    }

    public class ProjectorDefiner
    {
        private Type _projectionType;

        public static ProjectorDefiner For<TProjeciton>() => new ProjectorDefiner().From<TProjeciton>();

        public Projector<TConnection> Build<TConnection>()
        {
            var projector = (Projection<TConnection>)Activator.CreateInstance(_projectionType);
            return new Projector<TConnection>(Resolve.WhenEqualToHandlerMessageType(projector.Handlers));
        }

        public string GetProjectionName() => _projectionType.Name;

        private ProjectorDefiner From<TProjection>()
        {
            _projectionType = typeof(TProjection);
            return this;
        }
    }
}
