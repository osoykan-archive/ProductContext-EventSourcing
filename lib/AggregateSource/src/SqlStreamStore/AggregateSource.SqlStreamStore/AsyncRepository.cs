using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SqlStreamStore;
using SqlStreamStore.Streams;

namespace AggregateSource.SqlStreamStore
{
    public class AsyncRepository<TAggregateRoot> : IAsyncRepository<TAggregateRoot>
        where TAggregateRoot : IAggregateRootEntity
    {
        public AsyncRepository(
            Func<TAggregateRoot> rootFactory,
            ConcurrentUnitOfWork unitOfWork,
            IStreamStore eventStore,
            IEventDeserializer deserializer)
        {
            RootFactory = rootFactory ?? throw new ArgumentNullException(nameof(rootFactory));
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            EventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            EventDeserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        public Func<TAggregateRoot> RootFactory { get; }

        public ConcurrentUnitOfWork UnitOfWork { get; }

        public IStreamStore EventStore { get; }

        public IEventDeserializer EventDeserializer { get; }

        public async Task<TAggregateRoot> GetAsync(string identifier)
        {
            Optional<TAggregateRoot> result = await GetOptionalAsync(identifier);
            if (!result.HasValue) throw new AggregateNotFoundException(identifier, typeof(TAggregateRoot));

            return result.Value;
        }

        public async Task<Optional<TAggregateRoot>> GetOptionalAsync(string identifier)
        {
            if (UnitOfWork.TryGet(identifier, out Aggregate aggregate)) return new Optional<TAggregateRoot>((TAggregateRoot)aggregate.Root);

            var start = 0;
            const int batchSize = 500; // TODO: configurable in ReaderConfiguration

            ReadStreamPage page;
            var events = new List<StreamMessage>();

            do
            {
                page = await EventStore.ReadStreamForwards(identifier, start, batchSize);

                if (page.Status == PageReadStatus.StreamNotFound) return Optional<TAggregateRoot>.Empty;

                events.AddRange(page.Messages);

                start = page.NextStreamVersion;
            }
            while (!page.IsEnd);

            object[] deserializedEvents = await Task.WhenAll(
                events.Select(resolvedMsg => EventDeserializer.DeserializeAsync(resolvedMsg)
                ).ToArray());

            TAggregateRoot root = RootFactory();
            root.Initialize(deserializedEvents);
            aggregate = new Aggregate(identifier, page.LastStreamVersion, root);
            UnitOfWork.Attach(aggregate);

            return new Optional<TAggregateRoot>(root);
        }

        public void Add(string identifier, TAggregateRoot root)
        {
            UnitOfWork.Attach(new Aggregate(identifier, ExpectedVersion.NoStream, root));
        }
    }
}
