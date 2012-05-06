using System;
using System.Collections.Generic;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgCoordinateOperationMethod
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual bool Reverse { get; set; }

		public virtual string Formula { get; set; }

		public virtual string Example { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Deprecated { get; set; }

		public virtual IList<EpsgCoordinateOperation> UsedBy { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
