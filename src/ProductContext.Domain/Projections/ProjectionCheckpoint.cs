using DapperExtensions.Mapper;

namespace ProductContext.Domain.Projections
{
    public class ProjectionCheckpoint
    {
        public string Projection { get; set; }

        public string Position { get; set; }
    }

    public class ProjectionCheckpointMap : ClassMapper<ProjectionCheckpoint>
    {
        public ProjectionCheckpointMap()
        {
            Table("ProjectionCheckpoint");
            Schema("dbo");
            Map(x => x.Projection).Key(KeyType.Assigned);
            AutoMap();
        }
    }
}
