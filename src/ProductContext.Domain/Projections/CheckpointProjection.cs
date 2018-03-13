using System.Data.SqlClient;

using Dapper;

using Projac;

namespace ProductContext.Domain.Projections
{
    public class CreateCheckpointSchema
    {
    }

    public class DropCheckpointSchema
    {
    }

    public class CheckpointProjection : Projection<SqlConnection>
    {
        public CheckpointProjection()
        {
            When<DropCheckpointSchema>((connection, @event) =>
            {
                connection.Execute(
                    @"IF EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='[ProjectionPosition]' AND XTYPE='U')
                        DROP TABLE [dbo].[ProjectionPosition]");
            });

            When<CreateCheckpointSchema>((connection, @event) =>
            {
                connection.Execute(@"IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='ProjectionPosition' AND XTYPE='U')
                        CREATE TABLE [dbo].[ProjectionPosition](
                            [Projection] NVARCHAR(100) NOT NULL PRIMARY KEY,
	                        [Position] BIGINT NOT NULL
                        ) ON [PRIMARY]");
            });
        }
    }
}
