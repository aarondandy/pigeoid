// TODO: source header

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class GeographicOffset :
		ITransformation<GeographicCoordinate>,
		ITransformation<GeographicHeightCoordinate>
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

		public void TransformValues([NotNull] GeographicCoordinate[] values) {
			for (int i = 0; i < values.Length; i++)
				TransformValue(ref values[i]);
		}

		private void TransformValue(ref GeographicCoordinate value) {
			value = new GeographicCoordinate(value.Latitude + DLat, value.Longitude + DLon);
		}

		public GeographicCoordinate TransformValue(GeographicCoordinate value) {
			return new GeographicCoordinate(value.Latitude + DLat, value.Longitude + DLon);
		}

		[NotNull] public IEnumerable<GeographicCoordinate> TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
			return new GeographicHeightCoordinate(value.Latitude + DLat, value.Longitude + DLon, value.Height);
		}

		private void TransformValue(ref GeographicHeightCoordinate value) {
			value = new GeographicHeightCoordinate(value.Latitude + DLat, value.Longitude + DLon, value.Height);
		}

		public void TransformValues([NotNull] GeographicHeightCoordinate[] values) {
			for (int i = 0; i < values.Length; i++)
				TransformValue(ref values[i]);
		}

		[NotNull] public IEnumerable<GeographicHeightCoordinate> TransformValues([NotNull] IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}

		ITransformation<GeographicCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicCoordinate>.GetInverse() {
			return GetInverse();
		}

		public bool HasInverse {
			[Pure, ContractAnnotation("=>true")] get { return true; }
		}

		[NotNull] GeographicOffset GetInverse() {
			return new GeographicOffset(-DLat, -DLon);
		}

		ITransformation<GeographicCoordinate> ITransformation<GeographicCoordinate>.GetInverse() {
			return GetInverse();
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		ITransformation<GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate>.GetInverse() {
			return GetInverse();
		}

		ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>.GetInverse() {
			return GetInverse();
		}

	}
}
