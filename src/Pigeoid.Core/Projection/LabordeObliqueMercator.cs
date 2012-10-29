using System;
using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class LabordeObliqueMercator : ProjectionBase
	{

		protected GeographicCoordinate ProjectionCenter;
		protected double Beta;
		protected double SLatitude;
		protected double R;
		protected double C;

		public LabordeObliqueMercator(
			GeographicCoordinate projectionCenter,
			double azimuthOfInitialLine,
			double scaleFactor,
			[NotNull] ISpheroid<double> spheroid,
			Vector2 falseProjectedOffset
		) : base(falseProjectedOffset, spheroid)
		{
			ProjectionCenter = projectionCenter;
			var cosLatCenter = Math.Cos(projectionCenter.Latitude);
			var sinLatCenter = Math.Sin(projectionCenter.Latitude);
			var oneMinusESq = 1 - ESq;
			var eSinLatCenter = E*sinLatCenter;
			Beta = Math.Sqrt(
				(
					(ESq*cosLatCenter*cosLatCenter*cosLatCenter*cosLatCenter)
					/oneMinusESq
				)
				+ 1);
			SLatitude = Math.Asin(Math.Sin(projectionCenter.Latitude)/Beta);
			R = MajorAxis * scaleFactor * (Math.Sqrt(oneMinusESq) / (1 - (ESq * sinLatCenter * sinLatCenter)));
			C = Math.Log(Math.Tan(QuarterPi + (SLatitude / 2.0)))
				- (
					Beta
					* Math.Log(
						Math.Tan(QuarterPi + (projectionCenter.Latitude / 2.0))
						* Math.Pow(
							(1 - eSinLatCenter) / (1 + eSinLatCenter),
							EHalf
						)
					)
				);
		}


		public override Point2 TransformValue(GeographicCoordinate source)
		{
			var sinLat = Math.Sin(source.Latitude);
			var halfLat = source.Latitude/2.0;
			var betaLonDiff = Beta*(source.Longitude - ProjectionCenter.Longitude);
			var q = C + (
				Beta
				* Math.Log(
					Math.Tan(QuarterPi + halfLat)
					* Math.Pow((1 - sinLat) / (1 + sinLat), EHalf)
				)
			);

			throw new NotImplementedException("This is wrong. Either me or the spec example.");
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse()
		{
			throw new NotImplementedException();
		}

		public override bool HasInverse
		{
			get { throw new NotImplementedException(); }
		}

	}
}
