// TODO: source header

using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// A hyperbolic Cassini Soldner projection.
	/// </summary>
	public class HyperbolicCassiniSoldner :
		CassiniSoldner,
		IEquatable<HyperbolicCassiniSoldner>
	{

		protected readonly double OneMinusESqMajorAxisSq6;

		/// <summary>
		/// The inverse of a hyperbolic Cassini Soldner projection.
		/// </summary>
		private class Inverted : InvertedTransformationBase<HyperbolicCassiniSoldner,Point2,GeographicCoord>
		{

			public Inverted(HyperbolicCassiniSoldner core)
				: base(core) {
				if (0 == Core.MajorAxis || 0 == Core.OneMinusESq)
					throw new ArgumentException("Core cannot be inverted.");
			}

			public override GeographicCoord TransformValue(Point2 coord) {
				double latp = Core.NaturalOrigin.Latitude + ((coord.Y - Core.FalseProjectedOffset.Y) / 315320.0);
				double tanLatp = Math.Tan(latp);
				double t = tanLatp * tanLatp;
				double lesq = Math.Sin(latp); lesq = 1.0 - (Core.ESq * lesq * lesq);
				double d = ((coord.X - Core.FalseProjectedOffset.X) * Math.Sqrt(lesq)) / Core.MajorAxis;
				double d2 = d * d;
				double s = (1.0 + (3.0 * t)) * d2;
				return new GeographicCoord(
					latp - (((lesq * tanLatp) / Core.OneMinusESq) * d2 * (0.5 - (s / 24))),
					Core.NaturalOrigin.Longitude + (
						(d - (t * d2 * d * ((1.0 / 3.0) + (s / 15.0))))
						/ Math.Cos(latp)
					)
				);
			}

		}

		/// <summary>
		/// Constructs a new hyperbolic cassini soldner projection.
		/// </summary>
		/// <param name="naturalOrigin">The natural origin.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="spheroid">The spheroid.</param>
		public HyperbolicCassiniSoldner(
			GeographicCoord naturalOrigin,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(naturalOrigin, falseProjectedOffset, spheroid)
		{
			OneMinusESqMajorAxisSq6 = OneMinusESq * MajorAxis * MajorAxis * 6.0;
		}

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { return 0 != MajorAxis && 0 != OneMinusESq; }
		}

		public override Point2 TransformValue(GeographicCoord coord) {
			double c = Math.Cos(coord.Latitude);
			double a = (coord.Longitude - NaturalOrigin.Longitude) * c;
			c = (ESecSq * c * c);
			double tanLat = Math.Tan(coord.Latitude);
			double t = tanLat * tanLat;
			double a2 = a * a;
			double lesq = Math.Sin(coord.Latitude); lesq = 1.0 - (ESq * lesq * lesq);
			double v = MajorAxis / Math.Sqrt(lesq);
			double x =
				v * (a - (a2 * a * t * (
					(1.0 / 6.0)
					+ ((8.0 - t + (8.0 * c)) * a2 / 120.0)
				)));
			return new Point2(
				FalseProjectedOffset.X + x - (
					(x * x * x * lesq)
					/ OneMinusESqMajorAxisSq6
				),
				FalseProjectedOffset.Y + (
					(
						MajorAxis * (
							(MLineCoef1 * coord.Latitude)
							- (MLineCoef2 * Math.Sin(2.0 * coord.Latitude))
							+ (MLineCoef3 * Math.Sin(4.0 * coord.Latitude))
							- (MLineCoef4 * Math.Sin(6.0 * coord.Latitude))
						)
					)
					- MOrigin
					+ (v * tanLat * a2 * (
						0.5 + ((5 - t + (6.0 * c)) * a2 / 24.0)
					))
				)
			);
		}

		public override string Name {
			get { return "Hyperbolic Cassini Soldner"; }
		}

		public bool Equals(HyperbolicCassiniSoldner other) {
			return !ReferenceEquals(other, null)
				&& (
					NaturalOrigin.Equals(other.NaturalOrigin)
					&& FalseProjectedOffset.Equals(other.FalseProjectedOffset)
					&& Spheroid.Equals(other.Spheroid)
				)
			;
		}

		public override bool Equals(object obj) {
			return null != obj
				&& (ReferenceEquals(this, obj) || Equals(obj as HyperbolicCassiniSoldner));
		}

		public override int GetHashCode() {
			return NaturalOrigin.GetHashCode() ^ (FalseProjectedOffset.GetHashCode());
		}

	}
}
