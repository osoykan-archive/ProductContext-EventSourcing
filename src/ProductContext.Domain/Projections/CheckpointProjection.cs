using DapperExtensions.Mapper;

namespace ProductContext.Domain.Projections
{
    public class CheckpointProjection
    {
        public string Projection { get; set; }

        public string Position { get; set; }
    }

    public class CheckpointProjectionMap : AutoClassMapper<CheckpointProjection>
    {
        public CheckpointProjectionMap()
        {
            Table("ProjectionCheckpoint");
            Schema("dbo");
        }
    }
}
