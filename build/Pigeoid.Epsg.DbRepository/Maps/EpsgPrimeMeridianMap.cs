using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DbRepository.Maps
{
	public class EpsgPrimeMeridianMap : ClassMap<EpsgPrimeMeridian>
	{

		public EpsgPrimeMeridianMap() {
			Table("[Prime Meridian]");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Code).Column("PRIME_MERIDIAN_CODE");
			Map(x => x.Name).Column("PRIME_MERIDIAN_NAME");
			Map(x => x.GreenwichLon).Column("GREENWICH_LONGITUDE");
			References(x => x.Uom).Column("UOM_CODE");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
		}

	}
}
