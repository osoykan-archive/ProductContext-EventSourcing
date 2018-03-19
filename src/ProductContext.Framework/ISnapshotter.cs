using System;
using System.Threading.Tasks;

namespace ProductContext.Framework
{
    public interface ISnapshotter
    {
        bool ShouldTakeSnapshot(Type aggregateType);

        Task Take(string stream);
    }
}
