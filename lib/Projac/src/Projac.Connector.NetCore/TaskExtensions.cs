using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Projac.Connector.NetCore
{
    internal static class TaskExtensions
    {
        public static async Task ExecuteAsync(this IEnumerable<Task> enumerable, CancellationToken cancellationToken)
        {
            foreach (Task task in enumerable)
            {
                await task.ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested) return;
            }
        }
    }
}
