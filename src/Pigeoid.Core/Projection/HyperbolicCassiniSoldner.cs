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
		private class Inverted : InvertedTransformationBase<HyperbolicCassiniSoldner,Point2,GeographicCoordinate>
		{

			public Inverted(HyperbolicCassiniSoldner core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 coordinate) {
				var latp = Core.NaturalOrigin.Latitude + ((coordinate.Y - Core.FalseProjectedOffset.Y) / 315320.0);
				var tanLatp = Math.Tan(latp);
				var t = tanLatp * tanLatp;
				var lesq = Math.Sin(latp); lesq = 1.0 - (Core.ESq * lesq * lesq);
				var d = ((coordinate.X - Core.FalseProjectedOffset.X) * Math.Sqrt(lesq)) / Core.MajorAxis;
				var d2 = d * d;
				var s = (1.0 + (3.0 * t)) * d2;
				return new GeographicCoordinate(
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
			GeographicCoordinate naturalOrigin,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(naturalOrigin, falseProjectedOffset, spheroid)
		{
			OneMinusESqMajorAxisSq6 = OneMinusESq * MajorAxis * MajorAxis * 6.0;
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return base.HasInverse && 0 != OneMinusESq; }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public override Point2 TransformValue(GeographicCoordinate coordinate) {
			var c = Math.Cos(coordinate.Latitude);
			var a = (coordinate.Longitude - NaturalOrigin.Longitude) * c;
			c = (ESecSq * c * c);
			var tanLat = Math.Tan(coordinate.Latitude);
			var t = tanLat * tanLat;
			var a2 = a * a;
			var lesq = Math.Sin(coordinate.Latitude); lesq = 1.0 - (ESq * lesq * lesq);
			var v = MajorAxis / Math.Sqrt(lesq);
			var x =
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
							(MLineCoefficient1 * coordinate.Latitude)
							- (MLineCoefficient2 * Math.Sin(2.0 * coordinate.Latitude))
							+ (MLineCoefficient3 * Math.Sin(4.0 * coordinate.Latitude))
							- (MLineCoefficient4 * Math.Sin(6.0 * coordinate.Latitude))
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
