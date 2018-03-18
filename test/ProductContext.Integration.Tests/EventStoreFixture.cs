using System;
using System.Collections.Generic;

namespace ProductContext.Integration.Tests
{
    public class EventStoreFixture : IDisposable
    {
        private readonly IDisposable _docker;

        public EventStoreFixture()
        {
            _docker = DockerHelper.StartContainerAsync("eventstore/eventstore:latest", new List<int> { 2113, 1113 }).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _docker?.Dispose();
        }
    }
}
