
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgCrsVertical : EpsgCrsDatumBased, ICrsVertical
	{

		private readonly EpsgDatumVertical _datum;

		internal EpsgCrsVertical(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumVertical datum)
		: base(code,name,area,deprecated,cs) {
			_datum = datum;
		}

		public override EpsgDatum Datum { get { return _datum; } }

		public EpsgDatumVertical VerticalDatum { get { return _datum; } }

		IDatum ICrsVertical.Datum { get { return _datum; } }

		public EpsgUom Unit { get { return Axis.Unit; } }

		IUom ICrsVertical.Unit { get { return Unit; } }

		public EpsgAxis Axis { get { return CoordinateSystem.Axes.FirstOrDefault(); } }

		IAxis ICrsVertical.Axis { get { return Axis; } }
	}
}
