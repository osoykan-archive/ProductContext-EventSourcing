using System.Data.SqlClient;

using Dapper;

using Projac.Connector.NetCore;

namespace ProductContext.Domain.Projections
{
    public class CreateCheckpointSchema
    {
    }

    public class DropCheckpointSchema
    {
    }

    public class CheckpointProjector : ConnectedProjection<SqlConnection>
    {
        public CheckpointProjector()
        {
            When<DropCheckpointSchema>((connection, @event) =>
            {
                connection.Execute(
                    @"IF EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='[ProjectionPosition]' AND XTYPE='U')
                        DROP TABLE [dbo].[ProjectionPosition]");
            });

            When<CreateCheckpointSchema>((connection, @event) => { connection.Execute(@"IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='ProjectionPosition' AND XTYPE='U')
                        CREATE TABLE [dbo].[ProjectionPosition](
                            [Projection] NVARCHAR(100) NOT NULL PRIMARY KEY,
	                        [Position] BIGINT NOT NULL
                        ) ON [PRIMARY]"); });
        }
    }
}
