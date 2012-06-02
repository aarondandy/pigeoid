using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgDatumMap : ClassMap<EpsgDatum>
	{

		public EpsgDatumMap() {
			Table("Datum");
			Id(x => x.Code).Column("DATUM_CODE");
			Map(x => x.Name).Column("DATUM_NAME");
			Map(x => x.Type).Column("DATUM_TYPE");
			Map(x => x.OriginDescription).Column("ORIGIN_DESCRIPTION");
			Map(x => x.RealizationEpoch).Column("REALIZATION_EPOCH");
			References(x => x.Ellipsoid).Column("ELLIPSOID_CODE");
			References(x => x.PrimeMeridian).Column("PRIME_MERIDIAN_CODE");
			References(x => x.AreaOfUse).Column("AREA_OF_USE_CODE");
			Map(x => x.Scope).Column("DATUM_SCOPE");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
			ReadOnly();
		}

	}
}
