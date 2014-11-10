using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DbRepository.Maps
{
	public class EpsgAxisNameMap : ClassMap<EpsgAxisName>
	{

		public EpsgAxisNameMap() {
			Table("[Coordinate Axis Name]");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Code).Column("COORD_AXIS_NAME_CODE");
			Map(x => x.Name).Column("COORD_AXIS_NAME");
			Map(x => x.Description).Column("DESCRIPTION");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
		}

	}
}
