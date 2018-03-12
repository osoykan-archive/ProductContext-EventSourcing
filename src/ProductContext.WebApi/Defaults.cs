using System.Net;
using System.Threading.Tasks;

using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace ProductContext.WebApi
{
    public static class Defaults
    {
        public static async Task<IEventStoreConnection> GetConnection()
        {
            ConnectionSettings settings = ConnectionSettings.Create()
                                                            .SetDefaultUserCredentials(new UserCredentials("admin", "changeit")).Build();

            IEventStoreConnection connection = EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, 1113));
            await connection.ConnectAsync();
            return connection;
        }
    }
}
