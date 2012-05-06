using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgCoordOpPathItemMap : ClassMap<EpsgCoordOpPathItem>
	{

		public EpsgCoordOpPathItemMap() {
			Table("[Coordinate_Operation Path]");
			CompositeId()
				.KeyProperty(x => x.CatCode, "CONCAT_OPERATION_CODE")
				.KeyProperty(x => x.Step, "OP_PATH_STEP");
			References(x => x.Operation).Column("SINGLE_OPERATION_CODE");
		}

	}
}
