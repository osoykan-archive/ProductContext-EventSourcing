using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace AggregateSource.SqlStreamStore.Tests
{
    public class RepositoryScenarioBuilder
    {
        readonly List<Action<IStreamStore>> _eventStoreSchedule;
        readonly IStreamStore _connection;
        readonly List<Action<UnitOfWork>> _unitOfWorkSchedule;
        readonly List<Action<ConcurrentUnitOfWork>> _concurrentUnitOfWorkSchedule;
        readonly UnitOfWork _unitOfWork;
        readonly ConcurrentUnitOfWork _concurrentUnitOfWork;
        private readonly IEventDeserializer _deserializer;

        public RepositoryScenarioBuilder(IStreamStore connection)
        {
            _unitOfWork = new UnitOfWork();
            _connection = connection;
            _concurrentUnitOfWork = new ConcurrentUnitOfWork();
            _eventStoreSchedule = new List<Action<IStreamStore>>();
            _unitOfWorkSchedule = new List<Action<UnitOfWork>>();
            _concurrentUnitOfWorkSchedule = new List<Action<ConcurrentUnitOfWork>>();
            _deserializer = new EventDeserializer();
        }

        public RepositoryScenarioBuilder ScheduleAppendToStream(string stream, params object[] events)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (events == null) throw new ArgumentNullException("events");
            _eventStoreSchedule.Add(connection =>
                                    connection.AppendToStream(
                                        stream,
                                        ExpectedVersion.Any,
                                        events.Select(e =>
                                                      new NewStreamMessage(
                                                          Guid.NewGuid(),
                                                          e.GetType().TypeQualifiedName(),
                                                          JsonConvert.SerializeObject(e),
                                                          "\"metadata\"")).ToArray()).Wait()); // Wait() fixes race condition in test
            return this;
        }

        public RepositoryScenarioBuilder ScheduleDeleteStream(string stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            _eventStoreSchedule.Add(connection => connection.DeleteStream(stream).Wait());
            return this;
        }

        public RepositoryScenarioBuilder ScheduleAttachToUnitOfWork(Aggregate aggregate)
        {
            if (aggregate == null) throw new ArgumentNullException("aggregate");
            _unitOfWorkSchedule.Add(uow => uow.Attach(aggregate));
            _concurrentUnitOfWorkSchedule.Add(uow => uow.Attach(aggregate));
            return this;
        }

        public AsyncRepository<AggregateRootEntityStub> BuildForAsyncRepository()
        {
            ExecuteScheduledActions();
            return new AsyncRepository<AggregateRootEntityStub>(
                AggregateRootEntityStub.Factory,
                _concurrentUnitOfWork,
                _connection,
                _deserializer);
        }

        void ExecuteScheduledActions()
        {
            foreach (var action in _eventStoreSchedule)
            {
                action(_connection);
            }
            foreach (var action in _unitOfWorkSchedule)
            {
                action(_unitOfWork);
            }
            foreach (var action in _concurrentUnitOfWorkSchedule)
            {
                action(_concurrentUnitOfWork);
            }
        }
    }
}