using System;

namespace Pigeoid.Epsg.DbRepository
{
	public class EpsgCoordOpPathItem : IEquatable<EpsgCoordOpPathItem>
	{

		public virtual int CatCode { get; set; }

		public virtual int Step { get; set; }

		public virtual EpsgCoordinateOperation Operation { get; set; }

		public override bool Equals(object obj) {
			return Equals(obj as EpsgCoordOpPathItem);
		}

		public virtual bool Equals(EpsgCoordOpPathItem other) {
			if (ReferenceEquals(this, other))
				return true;
			if (ReferenceEquals(null, other))
				return false;

			return CatCode == other.CatCode
				&& Step == other.Step;
		}

		public override int GetHashCode() {
			return -CatCode ^ Step;
		}

		public override string ToString() {
			return String.Concat(CatCode, ':', Step);
		}

	}
}
