using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DbRepository.Maps
{
	public class EpsgAxisMap : ClassMap<EpsgAxis>
	{
		public EpsgAxisMap() {
            Table("[Coordinate Axis]");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Code).Column("COORD_AXIS_CODE");
			Map(x => x.Orientation).Column("COORD_AXIS_ORIENTATION");
			Map(x => x.Abbreviation).Column("COORD_AXIS_ABBREVIATION");
			References(x => x.Uom).Column("UOM_CODE");
			Map(x => x.OrderValue).Column("ORDER");
			References(x => x.NameObject).Column("COORD_AXIS_NAME_CODE");
			References(x => x.CoordinateSystem).Column("COORD_SYS_CODE");
		}
	}
}
