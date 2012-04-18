// TODO: source header

using System.Collections.Generic;
using System.Linq;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class Geographic2dTo3d :
		ITransformation<GeographicCoord, GeographicHeightCoord>
	{

		public readonly double Height;

		public Geographic2dTo3d() : this(0) { }

		public Geographic2dTo3d(double height) {
			Height = height;
		}

		public GeographicHeightCoord TransformValue(GeographicCoord value) {
			return new GeographicHeightCoord(value.Latitude, value.Longitude, Height);
		}

		public IEnumerable<GeographicHeightCoord> TransformValues(IEnumerable<GeographicCoord> values) {
			return values.Select(TransformValue);
		}

		public ITransformation<GeographicHeightCoord, GeographicCoord> GetInverse() {
			return new Geographic3dTo2d(Height);
		}

		public bool HasInverse {
			get { return true; }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}
	}

	public class Geographic3dTo2d :
		ITransformation<GeographicHeightCoord, GeographicCoord>
	{

		public readonly double Height;

		public Geographic3dTo2d() : this(0) { }

		public Geographic3dTo2d(double height) {
			Height = height;
		}

		public GeographicCoord TransformValue(GeographicHeightCoord value) {
			return new GeographicCoord(value.Latitude, value.Longitude);
		}

		public IEnumerable<GeographicCoord> TransformValues(IEnumerable<GeographicHeightCoord> values) {
			return values.Select(TransformValue);
		}

		public ITransformation<GeographicCoord, GeographicHeightCoord> GetInverse() {
			return new Geographic2dTo3d(Height);
		}

		public bool HasInverse {
			get { return true; }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}
	}
}
