// TODO: source header

using System.Collections.Generic;
using System.Linq;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class GeographicOffset : ITransformation<GeographicCoordinate>
	{

		public readonly double DLat;
		public readonly double DLon;

		public GeographicCoordinate D {
			get { return new GeographicCoordinate(DLat, DLon); }
		}

		public GeographicOffset(double dLat, double dLon) {
			DLat = dLat;
			DLon = dLon;
		}

		public GeographicOffset(GeographicCoordinate d)
			: this(d.Latitude, d.Longitude) { }

		public void TransformValues(GeographicCoordinate[] values) {
			for (int i = 0; i < values.Length; i++)
				TransformValue(ref values[i]);
		}

		private void TransformValue(ref GeographicCoordinate value) {
			value = new GeographicCoordinate(value.Latitude + DLat, value.Longitude + DLon);
		}

		public GeographicCoordinate TransformValue(GeographicCoordinate value) {
			return new GeographicCoordinate(value.Latitude + DLat, value.Longitude + DLon);
		}

		public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		ITransformation<GeographicCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicCoordinate>.GetInverse() {
			return GetInverse();
		}

		public bool HasInverse {
			get { return true; }
		}

		public ITransformation<GeographicCoordinate> GetInverse() {
			return new GeographicOffset(-DLat, -DLon);
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}
	}
}
