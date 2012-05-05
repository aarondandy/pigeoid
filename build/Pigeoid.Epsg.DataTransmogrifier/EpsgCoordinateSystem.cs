using System;
using System.Collections.Generic;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgCoordinateSystem
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual string TypeName { get; set; }

		public virtual int Dimension { get; set; }

		public virtual string Remarks { get; set; }

		public virtual string InformationSource { get; set; }

		public virtual string DataSource { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Deprecated { get; set; }

		public virtual IList<EpsgAxis> Axes { get; set; }

		public virtual IList<EpsgCrs> CrsUsage { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
