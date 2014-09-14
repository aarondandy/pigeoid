using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgCrsMap : ClassMap<EpsgCrs>
	{
		public EpsgCrsMap() {
			Table("[Coordinate Reference System]");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Code).Column("COORD_REF_SYS_CODE");
			Map(x => x.Name).Column("COORD_REF_SYS_NAME");
			References(x => x.Area).Column("AREA_OF_USE_CODE");
			Map(x => x.Kind).Column("COORD_REF_SYS_KIND");
			References(x => x.CoordinateSystem).Column("COORD_SYS_CODE");
			References(x => x.Datum).Column("DATUM_CODE");
			References(x => x.SourceGeographicCrs).Column("SOURCE_GEOGCRS_CODE");
			References(x => x.Projection).Column("PROJECTION_CONV_CODE");
			References(x => x.CompoundHorizontalCrs).Column("CMPD_HORIZCRS_CODE");
			References(x => x.CompoundVerticalCrs).Column("CMPD_VERTCRS_CODE");
			Map(x => x.Scope).Column("CRS_SCOPE");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Show).Column("SHOW_CRS");
			Map(x => x.Deprecated).Column("DEPRECATED");
			HasMany(x => x.OperationsFrom).KeyColumn("SOURCE_CRS_CODE").ReadOnly();
			HasMany(x => x.OperationsTo).KeyColumn("TARGET_CRS_CODE").ReadOnly();
		}
	}
}
