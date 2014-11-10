using System;

namespace Pigeoid.Epsg.DbRepository
{
	public class EpsgEllipsoid
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual double SemiMajorAxis { get; set; }

		public virtual EpsgUom Uom { get; set; }

		public virtual double? InverseFlattening { get; set; }

		public virtual double? SemiMinorAxis { get; set; }

		public virtual bool EllipsoidShape { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Deprecated { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
