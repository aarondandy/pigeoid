// TODO: source header

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class GeographicDimensionChange :
		ITransformation<GeographicCoordinate, GeographicHeightCoordinate>,
		ITransformation<GeographicHeightCoordinate, GeographicCoordinate>
	{

		private readonly double _defaultHeight;

		public GeographicDimensionChange() : this(0) { }

		public GeographicDimensionChange(double defaultHeight) {
			_defaultHeight = defaultHeight;
		}

		public double DefaultHeight { get { return _defaultHeight; } }

		public GeographicHeightCoordinate TransformValue(GeographicCoordinate value) {
			return new GeographicHeightCoordinate(value.Latitude, value.Longitude, _defaultHeight);
		}

		[NotNull] public IEnumerable<GeographicHeightCoordinate> TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public GeographicCoordinate TransformValue(GeographicHeightCoordinate value) {
			return new GeographicCoordinate(value.Latitude, value.Longitude);
		}

		[NotNull] public IEnumerable<GeographicCoordinate> TransformValues([NotNull] IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}

		bool ITransformation.HasInverse {
			get { return true; }
		}

		ITransformation<GeographicHeightCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.GetInverse() {
			return this;
		}

		ITransformation<GeographicCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.GetInverse() {
			return this;
		}

		ITransformation ITransformation.GetInverse() {
			return this;
		}

	}

}
