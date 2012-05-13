using System;
using System.Collections.Generic;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgArea
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual string AreaOfUse { get; set; }

		public virtual double? SouthBound { get; set; }

		public virtual double? NorthBound { get; set; }

		public virtual double? WestBound { get; set; }

		public virtual double? EastBound { get; set; }

		public virtual string Iso2 { get; set; }

		public virtual string Iso3 { get; set; }

		public virtual int IsoNCode { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Deprecated { get; set; }

		public virtual IList<EpsgCrs> CrsUsage { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
