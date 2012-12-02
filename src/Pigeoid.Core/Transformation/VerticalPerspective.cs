using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class VerticalPerspective :
		ITransformation<GeographicHeightCoordinate, Point2>,
		ITransformation<GeographicCoordinate, Point2>
	{

		protected readonly GeographicHeightCoordinate Origin;
		protected readonly double ViewPointHeight;
		protected readonly double OriginRadiusOfCurvature;
		protected readonly double SinLatOrigin;
		protected readonly double CosLatOrigin;
		protected readonly double ESq;
		protected readonly ISpheroid<double> Spheroid;

		public VerticalPerspective(GeographicHeightCoordinate origin, double viewPointHeight, ISpheroid<double> spheroid) {
			Origin = origin;
			ViewPointHeight = viewPointHeight;
			Spheroid = spheroid;
			SinLatOrigin = Math.Sin(origin.Latitude);
			CosLatOrigin = Math.Sin(origin.Latitude);
			ESq = spheroid.ESquared;
			OriginRadiusOfCurvature = spheroid.A / Math.Sqrt(1 - (ESq * SinLatOrigin * SinLatOrigin));

		}

		public Point2 TransformValue(GeographicHeightCoordinate value) {
			var lonDelta = value.Longitude - Origin.Longitude;
			var cosLonDelta = Math.Cos(lonDelta);
			var sinLonDelta = Math.Sin(lonDelta);
			var cosLat = Math.Cos(value.Latitude);
			var sinLat = Math.Sin(value.Latitude);
			var radiusOfCurvature = Spheroid.A / Math.Sqrt(1 - (ESq * sinLat * sinLat));
			var vh = radiusOfCurvature + value.Height;
			var eSqRadiusOriginOffsetThing = ESq * ((OriginRadiusOfCurvature * SinLatOrigin) - (radiusOfCurvature * sinLat));
			var u = vh * cosLat * sinLonDelta;
			var v = (vh * ((sinLat * CosLatOrigin) - (cosLat * SinLatOrigin * cosLonDelta))) + (eSqRadiusOriginOffsetThing * CosLatOrigin);
			var w = (vh * ((sinLat * SinLatOrigin) + (cosLat * CosLatOrigin * cosLonDelta))) + (eSqRadiusOriginOffsetThing * SinLatOrigin) - (OriginRadiusOfCurvature - Origin.Height);

			var viewPointHeightWDifference = ViewPointHeight - w;
			return new Point2(
				u * ViewPointHeight / viewPointHeightWDifference,
				v * ViewPointHeight / viewPointHeightWDifference
			);

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
