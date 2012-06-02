using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgParamValueMap : ClassMap<EpsgParamValue>
	{

		public EpsgParamValueMap() {
			Table("[Coordinate_Operation Parameter Value]");
			CompositeId()
				.KeyReference(x => x.Operation, "COORD_OP_CODE")
				.KeyReference(x => x.Method, "COORD_OP_METHOD_CODE")
				.KeyReference(x => x.Parameter, "PARAMETER_CODE");
			Map(x => x.NumericValue).Column("PARAMETER_VALUE");
			Map(x => x.TextValue).Column("PARAM_VALUE_FILE_REF");
			References(x => x.Uom).Column("UOM_CODE");
			ReadOnly();
		}

	}
}
