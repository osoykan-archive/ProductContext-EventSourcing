﻿using System;
using System.Linq;
using System.Linq.Expressions;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;
using AggregateSource.EventStore.Snapshots;

using Couchbase.Core;
using Couchbase.Linq;

using EventStore.ClientAPI;

using NodaTime;

using ProductContext.Domain.Contracts;
using ProductContext.Domain.Products;
using ProductContext.Domain.Projections;
using ProductContext.Framework;

namespace ProductContext.Integration.Tests
{
    public class ProductIntegrationTestBase
    {
        private static readonly GetSnapshotStreamName s_getSnapshotStreamName = (type, id) => $"{s_getStreamName(type, id)}-Snapshot";
        private static readonly GetStreamName s_getStreamName = (type, id) => $"{type.Name}-{id}";
        private static readonly Now s_now = () => SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

        public readonly Func<IBucket> GetBucket;
        public readonly AsyncRepository<Product> Repository;

        public ProductIntegrationTestBase()
        {
            IEventStoreConnection esConnection = Defaults.GetEsConnection("admin", "changeit", "tcp://admin:changeit@127.0.0.1:1113").GetAwaiter().GetResult();

            Bus = new InMemoryBus();
            var defaultSerializer = new DefaultEventDeserializer();
            var defaultSnapshotDeserializer = new DefaultSnapshotDeserializer();
            var concurrentUnitOfWork = new ConcurrentUnitOfWork();

            Repository = new AsyncRepository<Product>(
                Product.Factory,
                concurrentUnitOfWork,
                esConnection,
                new EventReaderConfiguration(
                    new SliceSize(500),
                    defaultSerializer,
                    new TypedStreamNameResolver(typeof(Product), s_getStreamName),
                    new NoStreamUserCredentialsResolver()));

            var productSnapshotableRepository = new AsyncSnapshotableRepository<Product>(
                Product.Factory,
                concurrentUnitOfWork,
                esConnection,
                new EventReaderConfiguration(new SliceSize(500), defaultSerializer, new TypedStreamNameResolver(typeof(Product), s_getStreamName), new NoStreamUserCredentialsResolver()),
                new AsyncSnapshotReader(esConnection, new SnapshotReaderConfiguration(defaultSnapshotDeserializer, new SnapshotableStreamNameResolver(typeof(Product), s_getSnapshotStreamName), new NoStreamUserCredentialsResolver())));

            DateTime GetDatetime() => DateTime.UtcNow;

            var productCommandHandlers = new ProductCommandHandlers(
                (type, id) => $"{type.Name}-{id}",
                Repository,
                productSnapshotableRepository,
                GetDatetime);

            Bus.Subscribe<Commands.V1.CreateProduct>(productCommandHandlers);
            Bus.Subscribe<Commands.V1.AddVariantToProduct>(productCommandHandlers);
            Bus.Subscribe<Commands.V1.AddContentToProduct>(productCommandHandlers);

            GetBucket = Defaults.GetCouchbaseBucket(nameof(ProductContext), "Administrator", "password", "http://localhost:8091");

            ProjectionManagerBuilder.With
                                    .Connection(esConnection)
                                    .Deserializer(new DefaultEventDeserializer())
                                    .CheckpointStore(new CouchbaseCheckpointStore(GetBucket))
                                    .Projections(
                                        ProjectorDefiner.For<ProductProjection>()
                ).Activate(GetBucket).GetAwaiter().GetResult();
        }

        public IBus Bus { get; }

        public void CreateIndex()
        {
            using (IBucket bucket = GetBucket())
            {
                bucket.Query<dynamic>("CREATE PRIMARY INDEX `ProductIndex` ON `ProductContext` USING GSI;");
            }
        }

        public ProductDocument Query(Expression<Func<ProductDocument, bool>> filter)
        {
            CreateIndex();
            ProductDocument doc;
            do
            {
                using (IBucket bucket = GetBucket())
                {
                    doc = new BucketContext(bucket).Query<ProductDocument>().FirstOrDefault(filter);
                }
            }
            while (doc == null);

            return doc;
        }

        public void WaitUntilProjected(Expression<Func<ProductDocument, bool>> filter)
        {
            CreateIndex();
            ProductDocument doc;
            do
            {
                using (IBucket bucket = GetBucket())
                {
                    doc = new BucketContext(bucket).Query<ProductDocument>().FirstOrDefault(filter);
                }
            }
            while (doc == null);
        }
    }
}
