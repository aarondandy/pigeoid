using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgNamingSystemMap : ClassMap<EpsgNamingSystem>
	{

		public EpsgNamingSystemMap() {
			Table("[Naming System]");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Code).Column("NAMING_SYSTEM_CODE");
			Map(x => x.Name).Column("NAMING_SYSTEM_NAME");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
		}

	}
}
