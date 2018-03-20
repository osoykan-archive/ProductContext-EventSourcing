using System;
using System.Text;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Snapshots;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace ProductContext.Framework
{
    public class EventStoreSnapshotter<TAggregate> : ISnapshotter where TAggregate : AggregateRootEntity
    {
        private readonly AsyncRepository<TAggregate> _repository;
        private readonly Func<string, string> _snapshotNameResolve;
        private readonly Func<ResolvedEvent, bool> _strategy;
        private readonly Now _now;

        public EventStoreSnapshotter(
            AsyncRepository<TAggregate> repository,
            Func<ResolvedEvent, bool> strategy,
            Func<string, string> snapshotNameResolve,
            Now now)
        {
            _repository = repository;
            _strategy = strategy;
            _snapshotNameResolve = snapshotNameResolve;
            _now = now;
        }

        public bool ShouldTakeSnapshot(Type aggregateType, ResolvedEvent e) =>
            typeof(ISnapshotable).IsAssignableFrom(aggregateType) && _strategy(e);

        public async Task Take(string stream)
        {
            TAggregate root = await _repository.GetAsync(stream);

            _repository.UnitOfWork.TryGet(stream, out Aggregate aggregate);

            object snapshot = new Snapshot(aggregate.ExpectedVersion, ((ISnapshotable)root).TakeSnapshot());

            var changes = new EventData(
                Guid.NewGuid(),
                snapshot.GetType().TypeQualifiedName(),
                true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(snapshot)),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EventMetadata()
                {
                    AggregateAssemblyQualifiedName = typeof(TAggregate).AssemblyQualifiedName,
                    AggregateType = typeof(TAggregate).Name,
                    TimeStamp = _now(),
                    IsSnapshot = true
                }))
            );

            string snapshotStream = _snapshotNameResolve(stream);
            await _repository.Connection.AppendToStreamAsync(snapshotStream, ExpectedVersion.Any, changes);
        }
    }
}
