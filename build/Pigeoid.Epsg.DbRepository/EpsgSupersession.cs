
namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgSupersession
	{

		public virtual int Id { get; set; }

		public virtual string TableName { get; set; }

		public virtual int ObjectCode { get; set; }

		public virtual int SupersededBy { get; set; }

		public virtual string Type { get; set; }

		public virtual int? Year { get; set; }

		public override string ToString() {
			return TableName + ", " + ObjectCode + " -> " + SupersededBy + " (" + Id + ')';
		}

	}
}
