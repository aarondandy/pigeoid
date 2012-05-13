
namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgAxis
	{

		public virtual int Code { get; set; }

		//public virtual int CoordinateSystemCode { get; set; }

		//public virtual int NameCode { get; set; }

		public virtual string Name { get {
			var no = NameObject;
			return null == no ? null : no.Name;
		} }

		public virtual EpsgAxisName NameObject { get; set; }

		public virtual string Orientation { get; set; }

		public virtual string Abbreviation { get; set; }

		public virtual EpsgUom Uom { get; set; }

		public virtual int OrderValue { get; set; }

		public virtual EpsgCoordinateSystem CoordinateSystem { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
