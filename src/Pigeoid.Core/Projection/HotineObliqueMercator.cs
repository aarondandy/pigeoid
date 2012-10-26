using System;
using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class HotineObliqueMercator : ProjectionBase
	{
		protected GeographicCoordinate GeographicCenter;
		protected double AzimuthOfInitialLine;
		protected double AngleFromRectified;
		protected double ScaleFactor;

		protected double B;
		protected double A;
		protected double TOrigin;
		protected double D;
		protected double F;
		protected double H;
		protected double G;
		protected double GammaOrigin;
		protected double LongitudeOrigin;
		protected bool NegativeCenterLatitude;
		protected bool HighAzimuth;

		public HotineObliqueMercator(GeographicCoordinate geographicCenter, double azimuthOfInitialLine, double angleFromRectified, double scaleFactor, Vector2 falseProjectedOffset, [NotNull] ISpheroid<double> spheroid)
			: base(falseProjectedOffset, spheroid)
		{
			GeographicCenter = geographicCenter;
			AzimuthOfInitialLine = azimuthOfInitialLine;
			AngleFromRectified = angleFromRectified;
			ScaleFactor = scaleFactor;
			var cosLatCenter = Math.Cos(geographicCenter.Latitude);
			var sinLatCenter = Math.Sin(geographicCenter.Longitude);
			var rootOfOneMinusESquared = Math.Sqrt(1 - ESq);

			HighAzimuth = AzimuthOfInitialLine >= HalfPi;

			B = Math.Sqrt(
				(
					(ESq * cosLatCenter * cosLatCenter * cosLatCenter * cosLatCenter)
					/ (1 - ESq)
				)
				+ 1.0
			);
			
			A = (MajorAxis * B * scaleFactor * rootOfOneMinusESquared)
				/ (1 - (ESq * sinLatCenter * sinLatCenter));
			
			TOrigin = Math.Tan(QuarterPi - (geographicCenter.Latitude / 2.0))
				/ Math.Pow(
					(1 - (E * sinLatCenter))
					/ (1 + (E * sinLatCenter))
				, E / 2.0);
			
			D = (B * rootOfOneMinusESquared)
				/(cosLatCenter * Math.Sqrt(1 - (ESq * sinLatCenter * sinLatCenter)));

			NegativeCenterLatitude = geographicCenter.Latitude < 0;
			F = D >= 1
				? D + Math.Sqrt((D * D) - 1)
				: D;
			if (NegativeCenterLatitude)
				F = -F;

			H = F*Math.Pow(TOrigin, B);

			G = (F - (1/F))/2.0;

			GammaOrigin = Math.Asin(Math.Sin(AzimuthOfInitialLine)/D);
			
			LongitudeOrigin = geographicCenter.Longitude - (Math.Asin(G*Math.Tan(GammaOrigin))/B);
			throw new NotImplementedException("This is all wrong.");
		}

		public override Point2 TransformValue(GeographicCoordinate source)
		{
			double uc;
			if(HighAzimuth)
			{
				uc = A*(GeographicCenter.Longitude - LongitudeOrigin);
			}
			else
			{
				uc = (A/B)*Math.Atan(Math.Sqrt((D*D) - 1.0)/Math.Cos(AzimuthOfInitialLine));
				if(NegativeCenterLatitude)
					uc = -uc;
			}

			var t = Math.Tan(QuarterPi - (source.Latitude / 2.0))
				/ Math.Pow((1 - (E * Math.Sin(source.Latitude))) / (1 + (E * Math.Sin(source.Latitude))), E);

			var q = H/Math.Pow(t, B);
			var bigS = (q - (1/q)) / 2;
			var bigT = (q + (1/q))/2;
			var bigV = Math.Sin(B*(source.Longitude - LongitudeOrigin));
			var bigU = ((-bigV * Math.Cos(GammaOrigin)) + (bigS * Math.Sin(GammaOrigin))) / bigT;

			var v = A * Math.Log((1 - bigU)/(1 + bigU)) / (2 * B);
			var u = A*Math.Atan(((bigS * Math.Cos(GammaOrigin)) + (bigV * Math.Sin(GammaOrigin)) ) / Math.Cos(B * (source.Longitude - LongitudeOrigin)))/B;

			throw new NotImplementedException("This is all wrong.");
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse()
		{
			throw new NotImplementedException();
		}

		public override bool HasInverse
		{
			get { throw new NotImplementedException(); }
		}

		public override string Name
		{
			get { return "Hotine Oblique Mercator"; }
		}
	}
}
