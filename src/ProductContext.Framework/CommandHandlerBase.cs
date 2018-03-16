using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;

using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

using Newtonsoft.Json;

namespace ProductContext.Framework
{
    public class CommandHandlerBase<T> where T : AggregateRootEntity
    {
        private readonly Func<DateTime> _getDateTime;
        private readonly GetStreamName _getStreamName;
        private readonly AsyncRepository<T> _repository;

        public CommandHandlerBase(GetStreamName getStreamName, AsyncRepository<T> repository, Func<DateTime> getDateTime)
        {
            _repository = repository;
            _getDateTime = getDateTime;
            _getStreamName = getStreamName;
        }

        protected async Task Add(Func<AsyncRepository<T>, Task> when)
        {
            await when(_repository);

            await AppendToStream(null);
        }

        protected async Task Update(string id, Func<T, Task> when)
        {
            string stream = _getStreamName(typeof(T), id);

            T loadedAggregate = await _repository.GetAsync(stream);

            await when(loadedAggregate).ConfigureAwait(false);

            await AppendToStream(stream);
        }

        private async Task AppendToStream(string stream)
        {
            foreach (Aggregate aggregate in _repository.UnitOfWork.GetChanges())
            {
                EventData[] changes = aggregate.Root.GetChanges()
                                               .Select(@event => new EventData(
                                                   Guid.NewGuid(),
                                                   @event.GetType().TypeQualifiedName(),
                                                   true,
                                                   Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                                                   Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
                                                   {
                                                       timeStamp = _getDateTime()
                                                   }))
                                                   )).ToArray();
                if (string.IsNullOrEmpty(stream))
                { 
                    stream = _getStreamName(typeof(T), aggregate.Identifier);
                }

                try
                {
                    await _repository.Connection.AppendToStreamAsync(stream, aggregate.ExpectedVersion, changes);
                }
                catch (WrongExpectedVersionException)
                {
                    StreamEventsSlice page = await _repository.Connection.ReadStreamEventsBackwardAsync(stream, -1, 1, false);
                    throw new WrongExpectedStreamVersionException(
                        $"Failed to append stream {stream} with expected version {aggregate.ExpectedVersion}. " +
                        $"{(page.Status == SliceReadStatus.StreamNotFound ? "Stream not found!" : $"Current Version: {page.LastEventNumber}")}");
                }
            }
        }
    }
}
