using System;

using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Services;

namespace ProductContext.Integration.Tests
{
    public class CouchbaseFixture : IDisposable
    {
        private readonly ICompositeService _image;
        private readonly IContainerService _runningImage;

        public CouchbaseFixture()
        {
            string resourcePath = $"{GetType().Assembly.GetName().Name}/{GetType().Assembly.GetName().Name}.couchbaseintegration/configure-node.txt";

            _image = new Builder().DefineImage("osoykan/couchbase")
                                  .From("couchbase:latest")
                                  .Maintainer("oguzhansoykan@gmail.com")
                                  .ExposePorts(8091, 8092, 8093, 8094, 11210)
                                  .Add($"emb:{resourcePath}", "/opt/couchbase/configure-node.sh")
                                  .Command("/opt/couchbase/configure-node.sh").Builder()
                                  .Build().Start();

            _runningImage = new Builder().UseContainer()
                                         .UseImage("osoykan/couchbase")
                                         .ExposePort(8091, 8091)
                                         .ExposePort(8092, 8092)
                                         .ExposePort(8093, 8093)
                                         .ExposePort(8094, 8094)
                                         .ExposePort(11210, 11210)
                                         .Build().Start();

            _runningImage.WaitForRunning();
        }

        public void Dispose()
        {
            _image?.Dispose();
            _runningImage?.Dispose();
        }
    }
}
