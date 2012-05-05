using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgAliasMap : ClassMap<EpsgAlias>
	{

		public EpsgAliasMap() {
			Id(x => x.Code).Column("ALIAS CODE");
			Map(x => x.ObjectTableName).Column("OBJECT_TABLE_NAME");
			Map(x => x.ObjectId).Column("OBJECT_CODE");
			Map(x => x.NamingSystemCode).Column("NAMING_SYSTEM_CODE");
			Map(x => x.Alias).Column("ALIAS");
			Map(x => x.Remarks).Column("REMARKS");
			Table("Alias");
		}

	}
}
