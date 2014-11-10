using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DbRepository.Maps
{
	public class EpsgCoordinateOperationMethodMap : ClassMap<EpsgCoordinateOperationMethod>
	{

		public EpsgCoordinateOperationMethodMap() {
			Table("[Coordinate_Operation Method]");
            ReadOnly();
            Cache.ReadOnly();
			Id(x => x.Code).Column("COORD_OP_METHOD_CODE");
			Map(x => x.Name).Column("COORD_OP_METHOD_NAME");
			Map(x => x.Reverse).Column("REVERSE_OP");
			Map(x => x.Formula).Column("FORMULA");
			Map(x => x.Example).Column("EXAMPLE");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
			HasMany(x => x.UsedBy).KeyColumn("COORD_OP_METHOD_CODE").ReadOnly();
			HasMany(x => x.ParamUse).KeyColumn("COORD_OP_METHOD_CODE").ReadOnly();
		}

	}
}
