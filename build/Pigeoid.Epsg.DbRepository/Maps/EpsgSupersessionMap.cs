using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DbRepository.Maps
{
	public class EpsgSupersessionMap : ClassMap<EpsgSupersession>
	{

		public EpsgSupersessionMap() {
			Table("Supersession");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Id).Column("SUPERSESSION_ID");
			Map(x => x.TableName).Column("OBJECT_TABLE_NAME");
			Map(x => x.ObjectCode).Column("OBJECT_CODE");
			Map(x => x.SupersededBy).Column("SUPERSEDED_BY");
			Map(x => x.Type).Column("SUPERSESSION_TYPE");
			Map(x => x.Year).Column("SUPERSESSION_YEAR");
		}

	}
}
