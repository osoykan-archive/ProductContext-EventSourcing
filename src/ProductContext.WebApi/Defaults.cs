using System;
using System.Threading.Tasks;

using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core;

using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace ProductContext.WebApi
{
    public static class Defaults
    {
        public static async Task<IEventStoreConnection> GetEsConnection(string username, string password, string uri)
        {
            ConnectionSettings settings = ConnectionSettings.Create()
                                                            .SetDefaultUserCredentials(new UserCredentials(username, password)).Build();

            IEventStoreConnection connection = EventStoreConnection.Create(settings, new Uri(uri));
            await connection.ConnectAsync();
            return connection;
        }

        public static Func<IBucket> GetCouchbaseBucket(string bucketName, string username = null, string password = null, params string[] uris)
        {
            var configuration = new ClientConfiguration();
            foreach (string uri in uris)
            {
                configuration.Servers.Add(new Uri(uri));
            }

            var cluster = new Cluster(configuration);
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                cluster.Authenticate(username, password);
            }

            return () => cluster.OpenBucket(bucketName);
        }
    }
}
