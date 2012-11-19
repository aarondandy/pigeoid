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

		public readonly GeographicHeightCoordinate _delta;

		public GeographicHeightCoordinate Delta {
			get { return _delta; }
		}

		public GeographicOffset(double deltaLat, double deltaLon)
			: this(deltaLat, deltaLon, 0) {}

		public GeographicOffset(double deltaLat, double deltaLon, double deltaHeight)
			: this(new GeographicHeightCoordinate(deltaLat, deltaLon, deltaHeight)) { }

		public GeographicOffset(GeographicHeightCoordinate delta){
			_delta = delta;
		}

		public GeographicOffset(GeographicCoordinate d)
			: this(d.Latitude, d.Longitude) { }

		public void TransformValues([NotNull] GeographicCoordinate[] values) {
			for (int i = 0; i < values.Length; i++)
				TransformValue(ref values[i]);
		}

		private void TransformValue(ref GeographicCoordinate value) {
			value = new GeographicCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude);
		}

		public GeographicCoordinate TransformValue(GeographicCoordinate value) {
			return new GeographicCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude);
		}

		[NotNull] public IEnumerable<GeographicCoordinate> TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
			return new GeographicHeightCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude, value.Height + Delta.Height);
		}

		private void TransformValue(ref GeographicHeightCoordinate value) {
			value = new GeographicHeightCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude, value.Height + Delta.Height);
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
			return new GeographicOffset(new GeographicHeightCoordinate(-Delta.Latitude, -Delta.Longitude, -Delta.Height));
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
