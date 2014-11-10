using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DbRepository.Maps
{
	public class EpsgCoordinateSystemMap : ClassMap<EpsgCoordinateSystem>
	{

		public EpsgCoordinateSystemMap() {
			Table("[Coordinate System]");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Code).Column("COORD_SYS_CODE");
			Map(x => x.Name).Column("COORD_SYS_NAME");
			Map(x => x.TypeName).Column("COORD_SYS_TYPE");
			Map(x => x.Dimension).Column("DIMENSION");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
			HasMany(x => x.Axes).KeyColumn("COORD_SYS_CODE").ReadOnly();
			HasMany(x => x.CrsUsage).KeyColumn("COORD_SYS_CODE").ReadOnly();
		}

	}
}
