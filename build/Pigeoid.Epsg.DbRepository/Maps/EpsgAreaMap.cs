using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgAreaMap : ClassMap<EpsgArea>
	{

		public EpsgAreaMap() {
			Id(x => x.Code).Column("AREA_CODE");
			Map(x => x.Name).Column("AREA_NAME");
			Map(x => x.AreaOfUse).Column("AREA_OF_USE");
			Map(x => x.SouthBound).Column("AREA_SOUTH_BOUND_LAT");
			Map(x => x.NorthBound).Column("AREA_NORTH_BOUND_LAT");
			Map(x => x.WestBound).Column("AREA_WEST_BOUND_LON");
			Map(x => x.EastBound).Column("AREA_EAST_BOUND_LON");
			Map(x => x.Iso2).Column("ISO_A2_CODE");
			Map(x => x.Iso3).Column("ISO_A3_CODE");
			Map(x => x.IsoNCode).Column("ISO_N_CODE");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
			HasMany(x => x.CrsUsage).KeyColumn("AREA_OF_USE_CODE");
			Table("Area");
		}

	}
}
