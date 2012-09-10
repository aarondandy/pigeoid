// TODO: source header

using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class ObliqueStereographic : ProjectionBase
	{

		private class Inverted : InvertedTransformationBase<ObliqueStereographic,Point2,GeographicCoordinate>
		{

			public Inverted(ObliqueStereographic core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 source) {
				throw new NotImplementedException();
			}
		}

		protected readonly GeographicCoordinate GeographicOrigin;
		protected readonly double ScaleFactor;
		protected readonly double R;
		protected readonly double N;
		protected readonly double C;
		protected readonly double SinChiOrigin;
		protected readonly double CosChiOrigin;

		public ObliqueStereographic(
			GeographicCoordinate geographicOrigin,
			double scaleFactor,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		) : base(falseProjectedOffset, spheroid)
		{
			GeographicOrigin = geographicOrigin;
			ScaleFactor = scaleFactor;
			var sinLat = Math.Sin(GeographicOrigin.Latitude);
			var cosLat = Math.Cos(GeographicOrigin.Latitude);
			var oneMinusESquareSinLatSquare = 1 - (ESq * sinLat * sinLat);
			var po = (MajorAxis * (1 - ESq)) / Math.Pow(oneMinusESquareSinLatSquare, 1.5);
			var vo = MajorAxis / Math.Sqrt(oneMinusESquareSinLatSquare);
			R = Math.Sqrt(po * vo);
			N = Math.Sqrt(1 + ((ESq * cosLat * cosLat * cosLat * cosLat)/(1 - ESq)));
			var s1 = (1 + sinLat)/(1 - sinLat);
			var s2 = (1 - (E * sinLat))/(1 + (E * sinLat));
			var w1 = Math.Pow(s1*Math.Pow(s2, E), N);
			SinChiOrigin = (w1 - 1)/(w1 + 1);
			C = (N + sinLat) * (1 - SinChiOrigin) / ((N - sinLat) * (1 + SinChiOrigin));
			var w2 = C*w1;
			var chiOrigin = Math.Asin((w2 - 1)/(w2 + 1));
			CosChiOrigin = Math.Cos(chiOrigin);
		}

		public override Point2 TransformValue(GeographicCoordinate source)
		{
			var lambda = (N*(source.Longitude - GeographicOrigin.Longitude)) + GeographicOrigin.Longitude;
			var lambdaDelta = lambda - GeographicOrigin.Longitude;
			var sinLat = Math.Sin(GeographicOrigin.Latitude);
			var eSinLat = E*sinLat;
			var sa = (1 + sinLat) / (1 - sinLat);
			var sb = (1 - eSinLat) / (1 + eSinLat);
			var w1 = Math.Pow(sa*Math.Pow(sb, E), N);
			var sinChi = (w1 - 1)/(w1 + 1);
			var c = (N + sinLat)*(1 - sinChi)/((N - sinLat)*(1 + sinChi));
			var w2 = c*w1;
			var chi = Math.Asin((w2 - 1)/(w2 + 1));
			var cosChi = Math.Cos(chi);
			
			var beta = 1
				+ (sinChi * SinChiOrigin)
				+ (cosChi * CosChiOrigin * Math.Cos(lambdaDelta)
			);
			var x = (2.0 * R * ScaleFactor * cosChi * Math.Sin(lambdaDelta) / beta)
				+ FalseProjectedOffset.X;
			var y = (2.0 * R * ScaleFactor * (
				(sinChi * CosChiOrigin)
				- (cosChi * sinChi * Math.Cos(lambdaDelta))
			) / beta) + FalseProjectedOffset.Y;
			throw new NotImplementedException("This is way off from the example.");
			return new Point2(x,y);
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { throw new NotImplementedException(); }
		}

		public override string Name {
			get { return "Oblique Stereographic"; }
		}
	}
}
