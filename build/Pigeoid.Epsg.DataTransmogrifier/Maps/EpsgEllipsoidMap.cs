using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgEllipsoidMap : ClassMap<EpsgEllipsoid>
	{

		public EpsgEllipsoidMap() {
			Table("Ellipsoid");
			Id(x => x.Code).Column("ELLIPSOID_CODE");
			Map(x => x.Name).Column("ELLIPSOID_NAME");
			Map(x => x.SemiMajorAxis).Column("SEMI_MAJOR_AXIS");
			References(x => x.Uom).Column("UOM_CODE");
			Map(x => x.InverseFlattening).Column("INV_FLATTENING");
			Map(x => x.SemiMinorAxis).Column("SEMI_MINOR_AXIS");
			Map(x => x.EllipsoidShape).Column("ELLIPSOID_SHAPE");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
		}

	}
}
