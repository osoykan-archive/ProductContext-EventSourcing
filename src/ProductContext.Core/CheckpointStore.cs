using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ProductContext.Framework
{
    public class CheckpointStore : ICheckpointStore
    {
        private readonly Func<SqlConnection> _getConnection;

        public CheckpointStore(Func<SqlConnection> getConnection)
        {
            _getConnection = getConnection;
        }

        public async Task<long> GetLastCheckpoint(string projection)
        {
            using (SqlConnection conn = _getConnection())
            {
                await conn.OpenAsync();
                SqlCommand command = conn.CreateCommand();
                using (command)
                {
                    command.CommandText = $"select Position from ProjectionPosition(nolock) where Projection = '{projection}'";
                    SqlDataReader position = await command.ExecuteReaderAsync();
                    if (position.HasRows && long.TryParse(position["Position"].ToString(), out long p))
                    {
                        return p;
                    }

                    return 0;
                }
            }
        }
    }

    public interface ICheckpointStore
    {
        Task<long> GetLastCheckpoint(string projection);
    }
}

