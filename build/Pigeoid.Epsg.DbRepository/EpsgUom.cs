using System;

namespace Pigeoid.Epsg.DbRepository
{
	public class EpsgUom
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual string Type { get; set; }

		public virtual EpsgUom RefUom { get; set; }

		public virtual double? FactorB { get; set; }

		public virtual double? FactorC { get; set; }

		/// <remarks>
		/// Remarks for UoM can provide some important other conversion routes.
		/// </remarks>
		public virtual string Remarks { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Deprecated { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
