//using System;
//using System.Threading.Tasks;
//using AggregateSource.SqlStreamStore.IntegrationTests.Framework;
//using NUnit.Framework;
//using SqlStreamStore;
//using SqlStreamStore.Streams;
//using StreamStoreStore.Json;

//namespace AggregateSource.SqlStreamStore.IntegrationTests.InMemory
//{
//    // ReSharper disable UnusedVariable
//    [TestFixture, SingleThreaded]
//    public class Construction
//    {
//        [Test]
//        public async Task FactoryCanNotBeNull()
//        {
//            using (var fixture = new InMemoryStreamStoreFixture())
//            {
//                using (var store = await fixture.GetStreamStore())
//                {
//                    Assert.Throws<ArgumentNullException>(
//                        () => new AsyncRepository<AggregateRootEntityStub>(
//                            null,
//                            new ConcurrentUnitOfWork(),
//                            store,
//                            new EventDeserializer()));
//                }
//            }
//        }

//        [Test]
//        public async Task ConcurrentUnitOfWorkCanNotBeNull()
//        {
//            using (var fixture = new InMemoryStreamStoreFixture())
//            {
//                using (var store = await fixture.GetStreamStore())
//                {
//                    Assert.Throws<ArgumentNullException>(() =>
//                        new AsyncRepository<AggregateRootEntityStub>(
//                            AggregateRootEntityStub.Factory, null,
//                            store,
//                            new EventDeserializer()));
//                }
//            }
//        }

//        [Test]
//        public void EventStoreConnectionCanNotBeNull()
//        {
//            Assert.Throws<ArgumentNullException>(() =>
//                new AsyncRepository<AggregateRootEntityStub>(
//                    AggregateRootEntityStub.Factory,
//                    new ConcurrentUnitOfWork(),
//                    null,
//                    new EventDeserializer()));
//        }

//        [Ignore("TODO after merge - requires setup")]
//        public void UsingCtorReturnsInstanceWithExpectedProperties()
//        {
//        }
//    }

//    [TestFixture, SingleThreaded]
//    public class WithEmptyStoreAndEmptyUnitOfWork
//    {
//        private AsyncRepository<AggregateRootEntityStub> _sut;
//        private StreamStoreAcceptanceTestFixture _fixture;
//        private IStreamStore _store;
//        private AggregateRootEntityStub _root;
//        private Model _model;

//        [SetUp]
//        public async Task SetUp()
//        {
//            _model = new Model();
//            _root = AggregateRootEntityStub.Factory();
//            _fixture = new InMemoryStreamStoreFixture();
//            _store = await _fixture.GetStreamStore();
//            _sut = new RepositoryScenarioBuilder()
//                .BuildForAsyncRepository(_store);
//        }

//        [Test]
//        public void GetAsyncThrows()
//        {
//            var exception =
//                Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
//                {
//                    var _ = await _sut.GetAsync(_model.UnknownIdentifier);
//                });
//            Assert.That(exception.Identifier, Is.EqualTo(_model.UnknownIdentifier));
//            Assert.That(exception.ClrType, Is.EqualTo(typeof(AggregateRootEntityStub)));
//        }

//        [Test]
//        public async Task GetOptionalAsyncReturnsEmpty()
//        {
//            var result = await _sut.GetOptionalAsync(_model.UnknownIdentifier);

//            Assert.That(result, Is.EqualTo(Optional<AggregateRootEntityStub>.Empty));
//        }

//        [Test]
//        public void AddAttachesToUnitOfWork()
//        {
//            _sut.Add(_model.KnownIdentifier, _root);

//            Aggregate aggregate;
//            var result = _sut.UnitOfWork.TryGet(_model.KnownIdentifier, out aggregate);
//            Assert.That(result, Is.True);
//            Assert.That(aggregate.Identifier, Is.EqualTo(_model.KnownIdentifier));
//            Assert.That(aggregate.Root, Is.SameAs(_root));
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _fixture?.Dispose();
//            _store?.Dispose();
//        }
//    }

//    [TestFixture, SingleThreaded]
//    public class WithEmptyStoreAndFilledUnitOfWork
//    {
//        private AsyncRepository<AggregateRootEntityStub> _sut;
//        private StreamStoreAcceptanceTestFixture _fixture;
//        private IStreamStore _store;
//        private AggregateRootEntityStub _root;
//        private Model _model;

//        [SetUp]
//        public async Task SetUp()
//        {
//            _model = new Model();
//            _root = AggregateRootEntityStub.Factory();
//            _fixture = new InMemoryStreamStoreFixture();
//            _store = await _fixture.GetStreamStore();
//            _sut = new RepositoryScenarioBuilder()
//                .ScheduleAttachToUnitOfWork(new Aggregate(_model.KnownIdentifier, 0, _root))
//                .BuildForAsyncRepository(_store);
//        }

//        [Test]
//        public void GetAsyncThrowsForUnknownId()
//        {
//            var exception =
//                Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
//                {
//                    var _ = await _sut.GetAsync(_model.UnknownIdentifier);
//                });
//            Assert.That(exception.Identifier, Is.EqualTo(_model.UnknownIdentifier));
//            Assert.That(exception.ClrType, Is.EqualTo(typeof(AggregateRootEntityStub)));
//        }

//        [Test]
//        public async Task GetAsyncReturnsRootOfKnownId()
//        {

//            var result = await _sut.GetAsync(_model.KnownIdentifier);

//            Assert.That(result, Is.SameAs(_root));

//        }

//        [Test]
//        public async Task GetOptionalAsyncReturnsEmptyForUnknownId()
//        {
//            var result = await _sut.GetOptionalAsync(_model.UnknownIdentifier);

//            Assert.That(result, Is.EqualTo(Optional<AggregateRootEntityStub>.Empty));
//        }

//        [Test]
//        public async Task GetOptionalAsyncReturnsRootForKnownId()
//        {

//            var result = await _sut.GetOptionalAsync(_model.KnownIdentifier);

//            Assert.That(result, Is.EqualTo(new Optional<AggregateRootEntityStub>(_root)));
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _fixture?.Dispose();
//            _store?.Dispose();
//        }
//    }

//    [TestFixture, SingleThreaded]
//    public class WithStreamPresentInStore
//    {
//        private AsyncRepository<AggregateRootEntityStub> _sut;
//        private StreamStoreAcceptanceTestFixture _fixture;
//        private IStreamStore _store;
//        private Model _model;

//        [SetUp]
//        public async Task SetUp()
//        {
//            _model = new Model();
//            _fixture = new InMemoryStreamStoreFixture();
//            _store = await _fixture.GetStreamStore();
//            _sut = new RepositoryScenarioBuilder()
//                .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
//                .BuildForAsyncRepository(_store);
//        }

//        [Test]
//        public void Can_serialize_and_deserialize_an_event()
//        {
//            var eventStub = new EventStub(1);
//            var streamMessage = new NewStreamMessage(Guid.NewGuid(),
//                eventStub.GetType().AssemblyQualifiedName,
//                SimpleJson.SerializeObject(eventStub),
//                "\"metadata\"");

//            var jsonData = streamMessage.JsonData;
//            var data = SimpleJson.DeserializeObject<EventStub>(jsonData);

//            Assert.AreEqual(eventStub.Value, data.Value);
//        }

//        [Test]
//        public void GetAsyncThrowsForUnknownId()
//        {
//            var exception =
//                Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
//                {
//                    var _ = await _sut.GetAsync(_model.UnknownIdentifier);
//                });
//            Assert.That(exception.Identifier, Is.EqualTo(_model.UnknownIdentifier));
//            Assert.That(exception.ClrType, Is.EqualTo(typeof(AggregateRootEntityStub)));
//        }

//        [Test]
//        public async Task GetReturnsRootOfKnownId()
//        {

//            var result = await _sut.GetAsync(_model.KnownIdentifier);

//            Assert.That(result.RecordedEvents, Is.EquivalentTo(new[] { new EventStub(1) }));
//        }

//        [Test]
//        public async Task GetOptionalAsyncReturnsEmptyForUnknownId()
//        {
//            var result = await _sut.GetOptionalAsync(_model.UnknownIdentifier);

//            Assert.That(result, Is.EqualTo(Optional<AggregateRootEntityStub>.Empty));
//        }

//        [Test]
//        public async Task GetOptionalAsyncReturnsRootForKnownId()
//        {
//            var result = await _sut.GetOptionalAsync(_model.KnownIdentifier);

//            Assert.That(result.HasValue, Is.True);
//            Assert.That(result.Value.RecordedEvents, Is.EquivalentTo(new[] { new EventStub(1) }));
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _fixture?.Dispose();
//            _store?.Dispose();
//        }
//    }

//    [TestFixture, SingleThreaded]
//    public class WithDeletedStreamInStore
//    {
//        private AsyncRepository<AggregateRootEntityStub> _sut;
//        private StreamStoreAcceptanceTestFixture _fixture;
//        private IStreamStore _store;
//        private AggregateRootEntityStub _root;
//        private Model _model;

//        [SetUp]
//        public async Task SetUp()
//        {
//            _model = new Model();
//            _root = AggregateRootEntityStub.Factory();
//            _fixture = new InMemoryStreamStoreFixture();
//            _store = await _fixture.GetStreamStore();
//            _sut = new RepositoryScenarioBuilder()
//                .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
//                .ScheduleDeleteStream(_model.KnownIdentifier)
//                .BuildForAsyncRepository(_store);
//        }

//        [Test]
//        public void GetAsyncThrowsForUnknownId()
//        {
//            var exception =
//                Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
//                {
//                    var _ = await _sut.GetAsync(_model.UnknownIdentifier);
//                });
//            Assert.That(exception.Identifier, Is.EqualTo(_model.UnknownIdentifier));
//            Assert.That(exception.ClrType, Is.EqualTo(typeof(AggregateRootEntityStub)));
//        }

//        [Test]
//        public void GetAsyncThrowsForKnownId()
//        {
//            var exception =
//                Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
//                {
//                    var _ = await _sut.GetAsync(_model.KnownIdentifier);
//                });
//            Assert.That(exception.Identifier, Is.EqualTo(_model.KnownIdentifier));
//            Assert.That(exception.ClrType, Is.EqualTo(typeof(AggregateRootEntityStub)));
//        }

//        [Test]
//        public async Task GetOptionalAsyncReturnsEmptyForUnknownId()
//        {
//            var result = await _sut.GetOptionalAsync(_model.UnknownIdentifier);

//            Assert.That(result, Is.EqualTo(Optional<AggregateRootEntityStub>.Empty));
//        }

//        [Test]
//        public async Task GetOptionalAsyncReturnsEmptyForKnownId()
//        {
//            var result = await _sut.GetOptionalAsync(_model.KnownIdentifier);

//            Assert.That(result, Is.EqualTo(Optional<AggregateRootEntityStub>.Empty));
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _fixture?.Dispose();
//            _store?.Dispose();
//        }
//    }
//    // ReSharper restore UnusedVariable
//}