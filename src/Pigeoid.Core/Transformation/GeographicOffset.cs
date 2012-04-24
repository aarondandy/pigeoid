// TODO: source header

using System.Collections.Generic;
using System.Linq;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class GeographicOffset : ITransformation<GeographicCoord>
	{

		public readonly double DLat;
		public readonly double DLon;

		public GeographicCoord D {
			get { return new GeographicCoord(DLat, DLon); }
		}

		public GeographicOffset(double dLat, double dLon) {
			DLat = dLat;
			DLon = dLon;
		}

		public GeographicOffset(GeographicCoord d)
			: this(d.Latitude, d.Longitude) { }

		public void TransformValues(GeographicCoord[] values) {
			for (int i = 0; i < values.Length; i++)
				TransformValue(ref values[i]);
		}

		private void TransformValue(ref GeographicCoord value) {
			value = new GeographicCoord(value.Latitude + DLat, value.Longitude + DLon);
		}

		public GeographicCoord TransformValue(GeographicCoord value) {
			return new GeographicCoord(value.Latitude + DLat, value.Longitude + DLon);
		}

		public IEnumerable<GeographicCoord> TransformValues(IEnumerable<GeographicCoord> values) {
			return values.Select(TransformValue);
		}

		ITransformation<GeographicCoord, GeographicCoord> ITransformation<GeographicCoord, GeographicCoord>.GetInverse() {
			return GetInverse();
		}

		public bool HasInverse {
			get { return true; }
		}

		public ITransformation<GeographicCoord> GetInverse() {
			return new GeographicOffset(-DLat, -DLon);
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}
	}
}
