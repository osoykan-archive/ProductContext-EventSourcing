using System;
using System.Collections.Generic;

using Couchbase;
using Couchbase.Management;

namespace ProductContext.Integration.Tests
{
    public class CouchbaseFixture : IDisposable
    {
        private readonly Cluster _cluster;
        private readonly IDisposable _docker;
        private readonly IClusterManager _manager;

        public CouchbaseFixture()
        {
            _docker = DockerHelper.StartContainerAsync("couchbase:latest", new List<int> { 8091, 8092, 8093, 8094 }).GetAwaiter().GetResult();

            //var configuration = new ClientConfiguration();
            //configuration.Servers.Add(new Uri("http://localhost:8091"));
            //configuration.SetAuthenticator(new PasswordAuthenticator("Administrator", "123456"));

            //ClusterHelper.Initialize(configuration);
            //_cluster = ClusterHelper.Get();
            //_manager = _cluster.CreateManager("Administrator", "123456");
            //_manager.CreateBucket("ProductContext", 200, BucketTypeEnum.Memcached, ReplicaNumber.Zero, AuthType.None);
        }

        public void Dispose()
        {
            _cluster.Dispose();
            _manager.Dispose();
            _docker?.Dispose();
        }
    }
}
