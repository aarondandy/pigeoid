using System;
using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class VerticalPerspectiveOrthographic :
		ITransformation<GeographicHeightCoordinate, Point2>,
		ITransformation<GeographicCoordinate, Point2>
	{

		public Point2 TransformValue(GeographicHeightCoordinate value) {
			throw new NotImplementedException();
		}

		public IEnumerable<Point2> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}
		public Point2 TransformValue(GeographicCoordinate value) {
			return TransformValue(new GeographicHeightCoordinate(value.Latitude, value.Longitude, 0));
		}
		public IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		ITransformation<Point2, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, Point2>.GetInverse() {
			throw new InvalidOperationException("No inverse.");
		}
		ITransformation<Point2, GeographicCoordinate> ITransformation<GeographicCoordinate, Point2>.GetInverse() {
			throw new InvalidOperationException("No inverse.");
		}
		ITransformation ITransformation.GetInverse() {
			throw new InvalidOperationException("No inverse.");
		}
		bool ITransformation.HasInverse {
			get { return false; }
		}


	}
}
