// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// A Cassini Soldner projection.
	/// </summary>
	public class CassiniSoldner : ProjectionBase, IEquatable<CassiniSoldner>
	{
		protected readonly GeographicCoordinate NaturalOrigin;
		protected readonly double MLineCoefficient1;
		protected readonly double MLineCoefficient2;
		protected readonly double MLineCoefficient3;
		protected readonly double MLineCoefficient4;
		protected readonly double MLineCoefficient1Major;
		protected readonly double LatLineCoefficient1;
		protected readonly double LatLineCoefficient2;
		protected readonly double LatLineCoefficient3;
		protected readonly double LatLineCoefficient4;
		protected readonly double MOrigin;
		protected readonly double MOriginYOffset;
		protected readonly double OneMinusESq;
		protected readonly double ESecSq;
		protected readonly double SquareRootOfOneMinusESq;

		private class Inverted : InvertedTransformationBase<CassiniSoldner,Point2,GeographicCoordinate>
		{

			public Inverted(CassiniSoldner core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 coordinate) {
				var latp = (Core.MOriginYOffset + coordinate.Y) / Core.MLineCoefficient1Major;
				latp = (
					latp
					+ (Core.LatLineCoefficient1 * Math.Sin(2.0 * latp))
					+ (Core.LatLineCoefficient2 * Math.Sin(4.0 * latp))
					+ (Core.LatLineCoefficient3 * Math.Sin(6.0 * latp))
					+ (Core.LatLineCoefficient4 * Math.Sin(8.0 * latp))
				);
				var tanLatp = Math.Tan(latp);
				var t = tanLatp * tanLatp;
				var lesq = Math.Sin(latp); lesq = 1.0 - (lesq * lesq * Core.ESq);
				var d = ((coordinate.X - Core.FalseProjectedOffset.X) * Math.Sqrt(lesq)) / Core.MajorAxis;
				var d2 = d * d;
				var s = (1.0 + (3.0 * t)) * d2;
				return new GeographicCoordinate(
					latp - (
						((lesq * tanLatp) / Core.SquareRootOfOneMinusESq)
						* d2 * (0.5 - (s / 24.0))
					),
					Core.NaturalOrigin.Longitude + (
						(d - (t * d2 * d * ((1.0 / 3.0) + (s / 15.0))))
						/ Math.Cos(latp)
					)
				);
			}

		}

		/// <summary>
		/// Constructs a new cassini soldner projection.
		/// </summary>
		/// <param name="naturalOrigin">The natural origin of the projection.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="spheroid">The spheroid.</param>
		public CassiniSoldner(
			GeographicCoordinate naturalOrigin,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		) : base(falseProjectedOffset, spheroid) {
			NaturalOrigin = naturalOrigin;

			var e4 = ESq * ESq;
			var e6 = e4 * ESq;
			MLineCoefficient1 = 1.0 - (ESq / 4.0) - (3.0 * e4 / 64.0) - (5.0 * e6 / 256.0);
			MLineCoefficient1Major = MLineCoefficient1 * MajorAxis;
			MLineCoefficient2 = (3.0 * ESq / 8.0) + (3.0 * e4 / 32.0) + (45.0 * e6 / 1024.0);
			MLineCoefficient3 = (15.0 * e4 / 256.0) + (45.0 * e6 / 1024.0);
			MLineCoefficient4 = (35.0 * e6 / 3072.0);
			MOrigin = MajorAxis * (
				(MLineCoefficient1 * naturalOrigin.Latitude)
				- (MLineCoefficient2 * Math.Sin(2.0 * naturalOrigin.Latitude))
				+ (MLineCoefficient3 * Math.Sin(4.0 * naturalOrigin.Latitude))
				- (MLineCoefficient4 * Math.Sin(6.0 * naturalOrigin.Latitude))
			);
			MOriginYOffset = MOrigin - falseProjectedOffset.Y;
			OneMinusESq = (1.0 - ESq);
			ESecSq = spheroid.ESecondSquared;
			SquareRootOfOneMinusESq = Math.Sqrt(OneMinusESq);
			var ep = (1.0 - SquareRootOfOneMinusESq) / (1.0 + SquareRootOfOneMinusESq);
			var ep2 = ep * ep;
			var ep3 = ep2 * ep;
			var ep4 = ep3 * ep;
			LatLineCoefficient1 = (3.0 * ep / 2.0) - (27.0 * ep3 / 32.0);
			LatLineCoefficient2 = (21.0 * ep2 / 16.0) - (55.0 * ep4 / 32.0);
			LatLineCoefficient3 = (151.0 * ep3 / 96.0)/* - (1097.0 * ep4 / 512.0)*/;
			LatLineCoefficient4 = (1097 * ep4 / 512);
		}

		public override Point2 TransformValue(GeographicCoordinate coordinate) {
			var v = Math.Sin(coordinate.Latitude);
			v = MajorAxis / Math.Sqrt(1.0 - (ESq * v * v));
			var c = Math.Cos(coordinate.Latitude);
			var a = (coordinate.Longitude - NaturalOrigin.Longitude) * c;
			c = ESecSq * c * c;
			var tanLat = Math.Tan(coordinate.Latitude);
			var t = tanLat * tanLat;
			var a2 = a * a;

			var east = FalseProjectedOffset.X + (
				v * ( a - (((t * a * a2) * (20.0 + (a2 * (8.0 + (8.0 * c) - t)))) / 120.0))
			);
			var m = MajorAxis * (
				(MLineCoefficient1 * coordinate.Latitude)
				- (MLineCoefficient2 * Math.Sin(2.0 * coordinate.Latitude))
				+ (MLineCoefficient3 * Math.Sin(4.0 * coordinate.Latitude))
				- (MLineCoefficient4 * Math.Sin(6.0 * coordinate.Latitude))
			);
			var x =
				m
				- MOrigin
				+ ((v * tanLat * a2 * (12 + (a2 * (5.0 - t + (6.0 * c))))) / 24.0)
			;
			var north = FalseProjectedOffset.Y + x;

			return new Point2(east, north);

			/*return new Point2(
				FalseProjectedOffset.X + (
					v * (
						a
						- (
							a2 * a * t * (
								(1.0 / 6.0)
								+ (
									(
										8.0
										- t
										+ (8.0 * c)
									)
									* a2
									/ 120.0
								)
							)
						)
					)
				),
				FalseProjectedOffset.Y + (
					(
						MajorAxis * (
							(MLineCoefficient1 * coordinate.Lat)
							- (MLineCoefficient2 * System.Math.Sin(2.0 * coordinate.Lat))
							+ (MLineCoefficient3 * System.Math.Sin(4.0 * coordinate.Lat))
							- (MLineCoefficient4 * System.Math.Sin(6.0 * coordinate.Lat))
						)
					)
					- MOrigin + (
						v * tanLat * a2 * (
							0.5
							+ (
								(
									5
									- t
									+ (6.0 * c)
								)
								* a2
								/ 24.0
							)
						)
					)
				)
			);*/
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get {
// ReSharper disable CompareOfFloatsByEqualityOperator
				return 0 != MLineCoefficient1Major
					&& 0 != MajorAxis
					&& 0 != SquareRootOfOneMinusESq;
// ReSharper restore CompareOfFloatsByEqualityOperator
			}
		}

		public override string Name {
			get { return "Cassini Soldner"; }
		}

		public override IEnumerable<INamedParameter> GetParameters() {
			return base.GetParameters().Concat(new INamedParameter[] {
                new NamedParameter<double>(NamedParameter.NameLatitudeOfNaturalOrigin, NaturalOrigin.Latitude),
                new NamedParameter<double>(NamedParameter.NameLongitudeOfNaturalOrigin, NaturalOrigin.Longitude)
            });
		}


		public bool Equals(CassiniSoldner other) {
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
				&& (ReferenceEquals(this, obj) || Equals(obj as CassiniSoldner));
		}

		public override int GetHashCode() {
			return NaturalOrigin.GetHashCode() ^ (-FalseProjectedOffset.GetHashCode());
		}

	}
}
