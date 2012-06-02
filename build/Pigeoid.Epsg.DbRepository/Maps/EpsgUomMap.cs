using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgUomMap : ClassMap<EpsgUom>
	{

		public EpsgUomMap() {
			Table("[Unit of Measure]");
			Id(x => x.Code).Column("UOM_CODE");
			Map(x => x.Name).Column("UNIT_OF_MEAS_NAME");
			Map(x => x.Type).Column("UNIT_OF_MEAS_TYPE");
			References(x => x.RefUom).Column("TARGET_UOM_CODE");
			Map(x => x.FactorB).Column("FACTOR_B");
			Map(x => x.FactorC).Column("FACTOR_C");
			Map(x => x.Remarks).Column("REMARKS");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
			ReadOnly();
		}

	}
}
