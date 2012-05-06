

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgAlias
	{

		public virtual int Code { get; set; }

		public virtual string ObjectTableName { get; set; }

		public virtual int ObjectId { get; set; }

		public virtual int NamingSystemCode { get; set; }

		public virtual string Alias { get; set; }

		public override string ToString() {
			return Alias + " (" + Code + ')';
		}

	}
}
