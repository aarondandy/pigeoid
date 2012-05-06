using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgParameterMap : ClassMap<EpsgParameter>
	{

		public EpsgParameterMap() {
			Table("[Coordinate_Operation Parameter]");
			Id(x => x.Code).Column("PARAMETER_CODE");
			Map(x => x.Name).Column("PARAMETER_NAME");
			Map(x => x.Description).Column("DESCRIPTION");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
		}

	}
}
