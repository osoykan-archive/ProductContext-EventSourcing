using System.Data.SqlClient;

using Dapper;

using DapperExtensions;

using ProductContext.Domain.Products;
using ProductContext.Framework;

using Projac.Connector.NetCore;

namespace ProductContext.Domain.Projections
{
    public class CreateProductSchema
    {
    }

    public class DropProductSchema
    {
    }

    public class ProductProjector : ConnectedProjection<SqlConnection>
    {
        public ProductProjector()
        {
            When<Envelope<Events.V1.ProductCreated>>((connection, e) =>
            {
                connection.Insert(new ProductProjection
                {
                    ProductId = e.Message.ProductId,
                    BrandId = e.Message.BrandId,
                    GenderId = e.Message.GenderId,
                    AgeGroupId = e.Message.AgeGroupId,
                    Code = e.Message.ProductCode,
                    BusinessUnitId = e.Message.BusinessUnitId
                });
            });

            When<Envelope<SetProjectionPosition>>(async (connection, e) =>
            {
                ProjectionPosition pPosition = await connection.QueryFirstOrDefaultAsync<ProjectionPosition>("select * from ProjectionPosition where Projection = @projection",
                    new { projection = nameof(ProductProjection) });

                if (pPosition != null)
                {
                    pPosition.Position = e.Message.Position;
                    connection.Update(pPosition);
                }
                else
                {
                    connection.Insert(new ProjectionPosition
                    {
                        Position = e.Position,
                        Projection = nameof(ProductProjection)
                    });
                }
            });

            When<CreateProductSchema>((connection, @event) => { connection.Execute(@"IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='Product' AND XTYPE='U')
                        CREATE TABLE [dbo].[Product](
	                        [ProductId] NVARCHAR(36) NOT NULL,
	                        [Code] NVARCHAR(100) NOT NULL,
	                        [BrandId] int NOT NULL,
	                        [GenderId] int NOT NULL,
	                        [AgeGroupId] int NOT NULL,
                            [BusinessUnitId] int NOT NULL,
                         CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED 
                        (
	                        [ProductId] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY]"); });

            When<DropProductSchema>((connection, @event) =>
            {
                connection.Execute(
                    @"IF EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='Product' AND XTYPE='U')
                        DROP TABLE [dbo].[Product]");
            });
        }
    }
}
