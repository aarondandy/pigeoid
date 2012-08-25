// TODO: source header

using System.Collections.Generic;
using System.Linq;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class Geographic2DTo3D :
		ITransformation<GeographicCoordinate, GeographicHeightCoordinate>
	{

		public readonly double Height;

		public Geographic2DTo3D() : this(0) { }

		public Geographic2DTo3D(double height) {
			Height = height;
		}

		public GeographicHeightCoordinate TransformValue(GeographicCoordinate value) {
			return new GeographicHeightCoordinate(value.Latitude, value.Longitude, Height);
		}

		public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public ITransformation<GeographicHeightCoordinate, GeographicCoordinate> GetInverse() {
			return new Geographic3DTo2D(Height);
		}

		public bool HasInverse {
			get { return true; }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}
	}

	public class Geographic3DTo2D :
		ITransformation<GeographicHeightCoordinate, GeographicCoordinate>
	{

		public readonly double Height;

		public Geographic3DTo2D() : this(0) { }

		public Geographic3DTo2D(double height) {
			Height = height;
		}

		public GeographicCoordinate TransformValue(GeographicHeightCoordinate value) {
			return new GeographicCoordinate(value.Latitude, value.Longitude);
		}

		public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}

		public ITransformation<GeographicCoordinate, GeographicHeightCoordinate> GetInverse() {
			return new Geographic2DTo3D(Height);
		}

		public bool HasInverse {
			get { return true; }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}
	}
}
