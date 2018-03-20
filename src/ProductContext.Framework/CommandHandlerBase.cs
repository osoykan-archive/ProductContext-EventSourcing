using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Snapshots;

using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

using Newtonsoft.Json;

namespace ProductContext.Framework
{
    public class CommandHandlerBase<T> where T : AggregateRootEntity, ISnapshotable
    {
        private readonly GetStreamName _getStreamName;
        private readonly Now _now;
        private readonly AsyncRepository<T> _repository;
        private readonly AsyncSnapshotableRepository<T> _snapshotableRepository;

        public CommandHandlerBase(
            GetStreamName getStreamName,
            AsyncRepository<T> repository,
            AsyncSnapshotableRepository<T> snapshotableRepository,
            Now now)
        {
            _repository = repository;
            _now = now;
            _getStreamName = getStreamName;
            _snapshotableRepository = snapshotableRepository;
        }

        protected async Task Add(Func<AsyncRepository<T>, Task> when)
        {
            await when(_repository);

            await AppendToStream();
        }

        protected async Task Update(string id, Func<T, Task> when)
        {
            T aggreagate = await _snapshotableRepository.GetAsync(id);

            await when(aggreagate);

            await AppendToStream();
        }

        private async Task AppendToStream()
        {
            foreach (Aggregate aggregate in _repository.UnitOfWork.GetChanges())
            {
                EventData[] changes = aggregate.Root.GetChanges()
                                               .Select(@event => new EventData(
                                                   Guid.NewGuid(),
                                                   @event.GetType().TypeQualifiedName(),
                                                   true,
                                                   Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                                                   Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EventMetadata
                                                   {
                                                       TimeStamp = _now(),
                                                       AggregateType = typeof(T).Name,
                                                       AggregateAssemblyQualifiedName = typeof(T).AssemblyQualifiedName,
                                                       IsSnapshot = false
                                                   }))
                                                   )).ToArray();
                try
                {
                    await _repository.Connection.AppendToStreamAsync(_getStreamName(typeof(T), aggregate.Identifier), aggregate.ExpectedVersion, changes);
                }
                catch (WrongExpectedVersionException)
                {
                    StreamEventsSlice page = await _repository.Connection.ReadStreamEventsBackwardAsync(aggregate.Identifier, -1, 1, false);
                    throw new WrongExpectedStreamVersionException(
                        $"Failed to append stream {aggregate.Identifier} with expected version {aggregate.ExpectedVersion}. " +
                        $"{(page.Status == SliceReadStatus.StreamNotFound ? "Stream not found!" : $"Current Version: {page.LastEventNumber}")}");
                }
            }
        }
    }
}
