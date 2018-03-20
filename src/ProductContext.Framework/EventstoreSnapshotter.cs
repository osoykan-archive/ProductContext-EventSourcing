using System;
using System.Text;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace ProductContext.Framework
{
    public class EventStoreSnapshotter<TAggregate, TSnapshot> : ISnapshotter where TAggregate : AggregateRootEntity
    {
        private readonly Now _now;
        private readonly AsyncRepository<TAggregate> _repository;
        private readonly Func<string, string> _snapshotNameResolve;
        private readonly Func<ResolvedEvent, bool> _strategy;

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

            var changes = new EventData(
                Guid.NewGuid(),
                typeof(TSnapshot).TypeQualifiedName(),
                true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(((ISnapshotable)root).TakeSnapshot(), DefaultEventDeserializer.DefaultSettings)),
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
            await _repository.Connection.AppendToStreamAsync(snapshotStream, ExpectedVersion.Any, changes);
        }
    }
}
