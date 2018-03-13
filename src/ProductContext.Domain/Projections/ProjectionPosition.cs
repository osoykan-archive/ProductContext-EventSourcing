using DapperExtensions.Mapper;

namespace ProductContext.Domain.Projections
{
    public class ProjectionPosition
    {
        public string Projection { get; set; }

        public long Position { get; set; }
    }

    public class ProjectionCheckpointMap : ClassMapper<ProjectionPosition>
    {
        public ProjectionCheckpointMap()
        {
            Table("ProjectionPosition");
            Schema("dbo");
            Map(x => x.Projection).Key(KeyType.Assigned);
            AutoMap();
        }
    }
}
