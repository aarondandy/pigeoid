using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DbRepository.Maps
{
	public class EpsgDeprecationMap : ClassMap<EpsgDeprecation>
	{

		public EpsgDeprecationMap() {
			Table("Deprecation");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Id).Column("DEPRECATION_ID");
			Map(x => x.Date).Column("DEPRECATION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.TableName).Column("OBJECT_TABLE_NAME");
			Map(x => x.ObjectCode).Column("OBJECT_CODE");
			Map(x => x.ReplaceCode).Column("REPLACED_BY");
			Map(x => x.Reason).Column("DEPRECATION_REASON");
		}

	}
}
