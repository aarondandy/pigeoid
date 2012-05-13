using System;
using System.Collections.Generic;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgCrs
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual EpsgArea Area { get; set; }

		public virtual string Kind { get; set; }

		public virtual EpsgCoordinateSystem CoordinateSystem { get; set; }

		public virtual EpsgDatum Datum { get; set; }

		public virtual EpsgCrs SourceGeographicCrs { get; set; }

		public virtual EpsgCoordinateOperation Projection { get; set; }

		public virtual EpsgCrs CompoundHorizontalCrs { get; set; }

		public virtual EpsgCrs CompoundVerticalCrs { get; set; }

		public virtual string Scope { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Show { get; set; }

		public virtual bool Deprecated { get; set; }

		public virtual IList<EpsgCoordinateOperation> OperationsFrom { get; set; }

		public virtual IList<EpsgCoordinateOperation> OperationsTo { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
