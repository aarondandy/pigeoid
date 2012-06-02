using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgParamUseMap : ClassMap<EpsgParamUse>
	{

		public EpsgParamUseMap() {
			Table("[Coordinate_Operation Parameter Usage]");
			CompositeId()
				.KeyReference(x => x.Method, "COORD_OP_METHOD_CODE")
				.KeyReference(x => x.Parameter, "PARAMETER_CODE");
			Map(x => x.SortOrder).Column("SORT_ORDER");
			Map(x => x.SignReversalText).Column("PARAM_SIGN_REVERSAL");
			ReadOnly();
		}

	}
}
