using System;

namespace Pigeoid.Epsg.DbRepository
{
	public class EpsgNamingSystem
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Deprecated { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
