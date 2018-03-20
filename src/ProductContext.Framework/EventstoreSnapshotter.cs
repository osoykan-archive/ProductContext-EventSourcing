using System;
using System.Text;
using System.Threading.Tasks;

using AggregateSource;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace ProductContext.Framework
{
    public class EventStoreSnapshotter<TAggregate, TSnapshot> : ISnapshotter where TAggregate : Aggregate
    {
        private readonly Func<string, Task<TAggregate>> _getAggreagate;
        private readonly Func<IEventStoreConnection> _getConnection;
        private readonly Now _now;
        private readonly Func<string, string> _snapshotNameResolve;
        private readonly Func<ResolvedEvent, bool> _strategy;

        public EventStoreSnapshotter(
            Func<string, Task<TAggregate>> getAggreagate,
            Func<IEventStoreConnection> getConnection,
            Func<ResolvedEvent, bool> strategy,
            Func<string, string> snapshotNameResolve,
            Now now)
        {
            _strategy = strategy;
            _snapshotNameResolve = snapshotNameResolve;
            _now = now;
            _getConnection = getConnection;
            _getAggreagate = getAggreagate;
        }

        public bool ShouldTakeSnapshot(Type aggregateType, ResolvedEvent e) =>
            typeof(ISnapshotable).IsAssignableFrom(aggregateType) && _strategy(e);

        public async Task Take(string stream)
        {
            TAggregate aggregate = await _getAggreagate(stream);

            var changes = new EventData(
                Guid.NewGuid(),
                typeof(TSnapshot).TypeQualifiedName(),
                true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(((ISnapshotable)aggregate.Root).TakeSnapshot(), DefaultEventDeserializer.DefaultSettings)),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EventMetadata
                {
                    AggregateAssemblyQualifiedName = typeof(TAggregate).AssemblyQualifiedName,
                    AggregateType = typeof(TAggregate).Name,
                    TimeStamp = _now(),
                    IsSnapshot = true,
                    Version = aggregate.ExpectedVersion
                }, DefaultEventDeserializer.DefaultSettings))
                );

            string snapshotStream = _snapshotNameResolve(stream);
            await _getConnection().AppendToStreamAsync(snapshotStream, ExpectedVersion.Any, changes);
        }
    }
}
