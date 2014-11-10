using System;

namespace Pigeoid.Epsg.DbRepository
{
	public class EpsgDatum
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual string Type { get; set; }

		public virtual string OriginDescription { get; set; }

		public virtual int? RealizationEpoch { get; set; }

		public virtual EpsgEllipsoid Ellipsoid { get; set; }

		public virtual EpsgPrimeMeridian PrimeMeridian { get; set; }

		public virtual EpsgArea AreaOfUse { get; set; }

		public virtual string Scope { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Deprecated { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}

}
