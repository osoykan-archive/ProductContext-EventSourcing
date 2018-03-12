using System.Data.SqlClient;

using Dapper;

using DapperExtensions;

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
            When<EventProjected>(async (connection, @event) =>
            {
                ProjectionCheckpoint pCheckpoint = await connection.QueryFirstOrDefaultAsync<ProjectionCheckpoint>("select * from ProjectionCheckpoint where Projection = @projection",
                    new { projection = @event.Projection });

                if (pCheckpoint != null)
                {
                    pCheckpoint.Position = @event.Position;
                    connection.Update(pCheckpoint);
                }
                else
                {
                    connection.Insert(new ProjectionCheckpoint
                    {
                        Position = @event.Position,
                        Projection = @event.Projection
                    });
                }
            });

            When<DropCheckpointSchema>((connection, @event) =>
            {
                connection.Execute(
                    @"IF EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='[ProjectionCheckpoint]' AND XTYPE='U')
                        DROP TABLE [dbo].[ProjectionCheckpoint]");
            });

            When<CreateCheckpointSchema>((connection, @event) =>
            {
                connection.Execute(@"IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='ProjectionCheckpoint' AND XTYPE='U')
                        CREATE TABLE [dbo].[ProjectionCheckpoint](
                            [Projection] NVARCHAR(100) NOT NULL PRIMARY KEY,
	                        [Position] NVARCHAR(MAX) NOT NULL
                        ) ON [PRIMARY]");
            });
        }
    }
}
