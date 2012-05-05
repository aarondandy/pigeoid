using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Pigeoid.Epsg.DataTransmogrifier.Maps
{
	public class EpsgAxisNameMap : ClassMap<EpsgAxisName>
	{

		public EpsgAxisNameMap() {
			Table("[Coordinate Axis Name]");
			Id(x => x.Code).Column("COORD_AXIS_NAME_CODE");
			Map(x => x.Name).Column("COORD_AXIS_NAME");
			Map(x => x.Description).Column("DESCRIPTION");
			Map(x => x.Remarks).Column("REMARKS");
			Map(x => x.InformationSource).Column("INFORMATION_SOURCE");
			Map(x => x.DataSource).Column("DATA_SOURCE");
			Map(x => x.RevisionDate).Column("REVISION_DATE");
			Map(x => x.ChangeId).Column("CHANGE_ID");
			Map(x => x.Deprecated).Column("DEPRECATED");
		}

	}
}
