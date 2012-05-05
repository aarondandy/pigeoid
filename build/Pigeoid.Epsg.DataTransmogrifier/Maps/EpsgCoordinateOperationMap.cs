using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgCoordinateOperationMap : ClassMap<EpsgCoordinateOperation>
	{

		public EpsgCoordinateOperationMap() {
			Table("Coordinate_Operation");
			Id(x => x.Code).Column("COORD_OP_CODE");
			Map(x => x.Name).Column("COORD_OP_NAME");
			Map(x => x.TypeName).Column("COORD_OP_TYPE");
			References(x => x.SourceCrs).Column("SOURCE_CRS_CODE");
			References(x => x.TargetCrs).Column("TARGET_CRS_CODE");
			Map(x => x.TransformVersion).Column("COORD_TFM_VERSION");
			Map(x => x.Variant).Column("COORD_OP_VARIANT");
			References(x => x.Area).Column("AREA_OF_USE_CODE");
			Map(x => x.Scope).Column("COORD_OP_SCOPE");
			Map(x => x.Accuracy).Column("COORD_OP_ACCURACY");
			Map(x => x.MethodCode).Column("COORD_OP_METHOD_CODE");
			Map(x => x.SourceUomCode).Column("UOM_CODE_SOURCE_COORD_DIFF");
			Map(x => x.TargetUomCode).Column("UOM_CODE_TARGET_COORD_DIFF");
			Map(x => x.Remarks).Column("REMARKS");
			Map(x => x.InformationSource).Column("INFORMATION_SOURCE");
			Map(x => x.DataSource).Column("DATA_SOURCE");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.ShowOperation).Column("SHOW_OPERATION");
			Map(x => x.Deprecated).Column("DEPRECATED");
		}

	}
}
