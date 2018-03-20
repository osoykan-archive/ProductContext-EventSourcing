using System;
using System.Threading.Tasks;

using EventStore.ClientAPI;

namespace ProductContext.Framework
{
    public interface ISnapshotter
    {
        bool ShouldTakeSnapshot(Type aggregateType, ResolvedEvent e);

        Task Take(string stream);
    }
}
