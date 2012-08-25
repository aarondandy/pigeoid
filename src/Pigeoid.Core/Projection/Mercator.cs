// TODO: source header

using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// A Mercator projection, 1SP or 2SP.
	/// </summary>
	public class Mercator :
		ProjectionBase,
		IEquatable<Mercator>
	{

		protected readonly double CentralMeridian;
		protected readonly double ScaleFactor;
		protected readonly double Ak;
		protected readonly double LatLineCoefficient1;
		protected readonly double LatLineCoefficient2;
		protected readonly double LatLineCoefficient3;
		protected readonly double LatLineCoefficient4;

		private readonly string _name;

		private class Inverted : InvertedTransformationBase<Mercator,Point2,GeographicCoordinate>
		{

			public Inverted(Mercator core)
				: base(core) {
				if (!Core.HasInverse) throw new ArgumentException("Core cannot be inverted.");
			}

			public override GeographicCoordinate TransformValue(Point2 coordinate) {
				double x =
					HalfPi
					- (2.0 * Math.Atan(Math.Pow(Math.E, (Core.FalseProjectedOffset.Y - coordinate.Y) / Core.Ak)));
				return new GeographicCoordinate(
					(
						x
						+ (Core.LatLineCoefficient1 * Math.Sin(2.0 * x))
						+ (Core.LatLineCoefficient2 * Math.Sin(4.0 * x))
						+ (Core.LatLineCoefficient3 * Math.Sin(6.0 * x))
						+ (Core.LatLineCoefficient4 * Math.Sin(8.0 * x))
					),
					((coordinate.X - Core.FalseProjectedOffset.X) / Core.Ak) + Core.CentralMeridian
				);
			}

		}

		/// <summary>
		/// Constructs a Mercator projection from 2 standard parallels.
		/// </summary>
		/// <param name="geographicOrigin">The geographic origin.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="spheroid">The spheroid.</param>
		public Mercator(
			GeographicCoordinate geographicOrigin,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: this(
				geographicOrigin.Longitude,
				(
					Math.Cos(geographicOrigin.Latitude)
					/ Math.Sqrt(1.0 - (spheroid.ESquared * Math.Sin(geographicOrigin.Latitude) * Math.Sin(geographicOrigin.Latitude)))
				),
				falseProjectedOffset,
				spheroid
			)
		{
			_name = "Mercator 2SP";
		}

		/// <summary>
		/// Constructs a Mercator projection from 1 standard parallel.
		/// </summary>
		/// <param name="centralMeridian">The central meridian.</param>
		/// <param name="scaleFactor">The scale factor.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="spheroid">The spheroid.</param>
		public Mercator(
			double centralMeridian,
			double scaleFactor,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(falseProjectedOffset, spheroid)
		{
			CentralMeridian = centralMeridian;
			ScaleFactor = scaleFactor;
			Ak = MajorAxis * scaleFactor;
			double e4 = ESq * ESq;
			double e6 = e4 * ESq;
			double e8 = e6 * ESq;
			LatLineCoefficient1 = (ESq / 2.0) + (5 / 24.0 * e4) + (e6 / 12.0) + (13.0 / 360.0 * e8);
			LatLineCoefficient2 = (7.0 / 48.0 * e4) + (29.0 / 240.0 * e6) + (811.0 / 11520.0 * e8);
			LatLineCoefficient3 = (7.0 / 120.0 * e6) + (81.0 / 1120.0 * e8);
			LatLineCoefficient4 = (4279 / 161280.0 * e8);
			_name = "Mercator 1SP";
		}

		public override Point2 TransformValue(GeographicCoordinate coordinate) {
			double eSinLat = E * Math.Sin(coordinate.Latitude);
			return new Point2(
				FalseProjectedOffset.X + (Ak * (coordinate.Longitude - CentralMeridian)),
				FalseProjectedOffset.Y + (Ak * Math.Log(
					Math.Tan(QuarterPi + (coordinate.Latitude / 2.0))
					* Math.Pow((1.0 - eSinLat) / (1.0 + eSinLat), EHalf)
				))
			);
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return 0 != Ak; }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public override string Name {
			get { return _name; }
		}

		public bool Equals(Mercator other) {
			return !ReferenceEquals(other, null)
				&& (
// ReSharper disable CompareOfFloatsByEqualityOperator
					CentralMeridian == other.CentralMeridian
					&& ScaleFactor == other.ScaleFactor
// ReSharper restore CompareOfFloatsByEqualityOperator
					&& FalseProjectedOffset.Equals(other.FalseProjectedOffset)
					&& Spheroid.Equals(other.Spheroid)
				)
			;
		}

		public override bool Equals(object obj) {
			return null != obj
				&& (ReferenceEquals(this, obj) || Equals(obj as Mercator));
		}

		public override int GetHashCode() {
			return -CentralMeridian.GetHashCode() ^ -ScaleFactor.GetHashCode();
		}

	}
}
