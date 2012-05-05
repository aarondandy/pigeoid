using System;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgAxisName
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual string Description { get; set; }

		public virtual string Remarks { get; set; }

		public virtual string InformationSource { get; set; }

		public virtual string DataSource { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool Deprecated { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
