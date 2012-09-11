// TODO: source header

using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{

	public class PolarStereographicA : PolarStereographic
	{
		public PolarStereographicA(GeographicCoordinate geographicOrigin, double scaleFactor, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
			: base(geographicOrigin, falseProjectedOffset, spheroid)
		{
			ScaleFactor = scaleFactor;
		}
	}

	public class PolarStereographicB : PolarStereographic
	{
		protected readonly double Mf;

		public PolarStereographicB(GeographicCoordinate geographicOrigin, double standardParallel, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
			: base(geographicOrigin, falseProjectedOffset, spheroid)
		{
			var sinLat = Math.Sin(standardParallel);
			var cosLat = Math.Cos(standardParallel);
			var eSinLat = E * sinLat;
			double tf;
			if (Math.Abs(Math.Abs(standardParallel) - HalfPi) > Double.Epsilon)
			{
				if (IsNorth)
				{
					tf = Math.Tan(QuarterPi - (standardParallel/2.0))
						*Math.Pow((1 + eSinLat)/(1 - eSinLat), EHalf);
				}
				else
				{
					tf = Math.Tan(QuarterPi + (standardParallel/2.0))
						/Math.Pow((1 + eSinLat)/(1 - eSinLat), EHalf);
				}
				Mf = cosLat / Math.Sqrt(1 - (ESq * sinLat * sinLat));
				ScaleFactor = Mf * CrazyEValue / (2 * tf);
				// t * 2.0 * MajorAxis * ScaleFactor / CrazyEValue
			}
			else
			{
				tf = 1.0;
				Mf = cosLat / Math.Sqrt(1 - (ESq * sinLat * sinLat));
				ScaleFactor = 1.0;
				// t * 2.0 * MajorAxis / CrazyEValue;
			}
			

		}
	}

	public class PolarStereographicC : PolarStereographicB
	{
		public PolarStereographicC(GeographicCoordinate geographicOrigin, double standardParallel, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
			: base(geographicOrigin, standardParallel, falseProjectedOffset, spheroid)
		{
			var pf = Mf * MajorAxis;
			FalseProjectedOffset = new Vector2(FalseProjectedOffset.X, (IsNorth ? pf : -pf) + FalseProjectedOffset.Y);
		}
	}

	public abstract class PolarStereographic : ProjectionBase
	{

		private class Inverse : InvertedTransformationBase<PolarStereographic, Point2, GeographicCoordinate>
		{

			private readonly double Coefficient1;
			private readonly double Coefficient2;
			private readonly double Coefficient3;
			private readonly double Coefficient4;

			public Inverse(PolarStereographic core) : base(core)
			{
				var e2 = Core.ESq;
				var e4 = e2 * e2;
				var e6 = e4*e2;
				var e8 = e6*e2;
				Coefficient1 = (e2/2) + (e4*5.0/24.0) + (e6/12) + (e8*12.0/360.0);
				Coefficient2 = (e4*7.0/48.0) + (e6*29.0/240.0) + (e8*811/11520);
				Coefficient3 = (e6*7.0/120.0) + (e8*81/1120);
				Coefficient4 = e8*4279.0/161280.0;
			}

			public override GeographicCoordinate TransformValue(Point2 value)
			{
				var dx = value.X - Core.FalseProjectedOffset.X;
				var dy = value.Y - Core.FalseProjectedOffset.Y;
				var p = Math.Sqrt((dx*dx) + (dy*dy));
				var t = p*Core.CrazyEValue/(2.0*Core.MajorAxis*Core.ScaleFactor);
				var chi = 2.0*Math.Atan(t);
				var lon = Core.GeographicOrigin.Longitude;
				if (Core.IsNorth)
				{
					chi = HalfPi - chi;
					if(0 != dx)
						lon += Math.Atan2(dx, -dy);
				}
				else
				{
					chi = chi - HalfPi;
					if(0 != dx)
						lon += Math.Atan2(dx, dy);
				}

				var lat = chi
					+ (Coefficient1 * Math.Sin(2 * chi))
					+ (Coefficient2 * Math.Sin(4 * chi))
					+ (Coefficient3 * Math.Sin(6 * chi))
					+ (Coefficient4 * Math.Sin(8 * chi));
				return new GeographicCoordinate(lat, lon);
			}
		}

		protected readonly GeographicCoordinate GeographicOrigin;
		protected double ScaleFactor;
		protected readonly bool IsNorth;
		protected readonly double CrazyEValue;
		protected readonly double TwoMajorAxis;
		protected readonly double TopScale;

		public PolarStereographic(GeographicCoordinate geographicOrigin, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
			: base(falseProjectedOffset, spheroid)
		{
			TwoMajorAxis = MajorAxis*2.0;
			GeographicOrigin = geographicOrigin;
			IsNorth = geographicOrigin.Latitude > 0;
			var onePlusESq = 1 + ESq;
			var oneMinusESq = 1 - ESq;
			TopScale = Math.Sqrt(Math.Pow(onePlusESq, onePlusESq) * Math.Pow(oneMinusESq, oneMinusESq));

			CrazyEValue = Math.Sqrt(
				Math.Pow(1 + E, 1 + E)
				* Math.Pow(1 - E, 1 - E)
			);

			ScaleFactor = 1.0;



		}

		public override Point2 TransformValue(GeographicCoordinate source)
		{
			var theta = source.Longitude - GeographicOrigin.Longitude;
			var halfLat = source.Latitude/2.0;
			var sinLat = Math.Sin(source.Latitude);
			var eSinLat = E*sinLat;

			if(IsNorth)
			{
				var t = Math.Tan(QuarterPi - halfLat)
					* Math.Pow((1 + eSinLat) / (1 - eSinLat), EHalf);
				var p = t * 2.0 * MajorAxis * ScaleFactor / CrazyEValue;
				var de = p * Math.Sin(theta);
				var dn = p * Math.Cos(theta);
				return new Point2(de + FalseProjectedOffset.X, FalseProjectedOffset.Y - dn);
			}
			else
			{
				var t = Math.Tan(QuarterPi + halfLat)
					/ Math.Pow((1 + eSinLat) / (1 - eSinLat), EHalf);
				var p = t * 2.0 * MajorAxis * ScaleFactor / CrazyEValue;
				var de = p * Math.Sin(theta);
				var dn = p * Math.Cos(theta);
				return new Point2(de + FalseProjectedOffset.X, dn + FalseProjectedOffset.Y);
			}
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse()
		{
			return new Inverse(this);
		}

		public override string Name
		{
			get { return "Polar Stereographic"; }
		}

		public override bool HasInverse
		{
			get { return 0 != MajorAxis && 0 != ScaleFactor; }
		}
	}
}
