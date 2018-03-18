using System;

using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Services;

namespace ProductContext.Integration.Tests
{
    public class EventStoreFixture : IDisposable
    {
        private readonly IContainerService _image;

        public EventStoreFixture()
        {
            _image = new Builder().UseContainer()
                                  .UseImage("eventstore/eventstore:latest")
                                  .WithEnvironment("EVENTSTORE_EXT_HTTP_PORT=2113")
                                  .WithEnvironment("EVENTSTORE_EXT_TCP_PORT=1113")
                                  .ExposePort(2113, 2113)
                                  .ExposePort(1113, 1113)
                                  .Build().Start();

            _image.WaitForRunning();
        }

        public void Dispose()
        {
            _image?.Dispose();
        }
    }
}
