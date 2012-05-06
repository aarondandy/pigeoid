using System;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgDeprecation
	{

		public virtual int Id { get; set; }

		public virtual DateTime Date { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual string TableName { get; set; }

		public virtual int ObjectCode { get; set; }

		public virtual int ReplaceCode { get; set; }

		public virtual string Reason { get; set; }

		public override string ToString() {
			return TableName + ", " + ObjectCode + " -> " + ReplaceCode + " (" + Id + ')';
		}

	}
}
