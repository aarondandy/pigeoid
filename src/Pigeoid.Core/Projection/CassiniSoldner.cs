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
		protected readonly GeographicCoord NaturalOrigin;
		protected readonly double MLineCoef1;
		protected readonly double MLineCoef2;
		protected readonly double MLineCoef3;
		protected readonly double MLineCoef4;
		protected readonly double MLineCoef1Major;
		protected readonly double LatLineCoef1;
		protected readonly double LatLineCoef2;
		protected readonly double LatLineCoef3;
		protected readonly double LatLineCoef4;
		protected readonly double MOrigin;
		protected readonly double MOriginYOffset;
		protected readonly double OneMinusESq;
		protected readonly double ESecSq;
		protected readonly double OneMinusESqSqrt;

		private class Inverted : InvertedTransformationBase<CassiniSoldner,Point2,GeographicCoord>
		{

			public Inverted(CassiniSoldner core)
				: base(core) {
				if (0 == Core.MLineCoef1Major || 0 == Core.MajorAxis || 0 == Core.OneMinusESqSqrt)
					throw new ArgumentException("Core cannot be inverted.");
			}

			public override GeographicCoord TransformValue(Point2 coord) {
				double latp = (Core.MOriginYOffset + coord.Y) / Core.MLineCoef1Major;
				latp = (
					latp
					+ (Core.LatLineCoef1 * Math.Sin(2.0 * latp))
					+ (Core.LatLineCoef2 * Math.Sin(4.0 * latp))
					+ (Core.LatLineCoef3 * Math.Sin(6.0 * latp))
					+ (Core.LatLineCoef4 * Math.Sin(8.0 * latp))
				);
				double tanLatp = Math.Tan(latp);
				double t = tanLatp * tanLatp;
				double lesq = Math.Sin(latp); lesq = 1.0 - (lesq * lesq * Core.ESq);
				double d = ((coord.X - Core.FalseProjectedOffset.X) * Math.Sqrt(lesq)) / Core.MajorAxis;
				double d2 = d * d;
				double s = (1.0 + (3.0 * t)) * d2;
				return new GeographicCoord(
					latp - (
						((lesq * tanLatp) / Core.OneMinusESqSqrt)
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
			GeographicCoord naturalOrigin,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		) : base(falseProjectedOffset, spheroid) {
			NaturalOrigin = naturalOrigin;

			double e4 = ESq * ESq;
			double e6 = e4 * ESq;
			MLineCoef1 = 1.0 - (ESq / 4.0) - (3.0 * e4 / 64.0) - (5.0 * e6 / 256.0);
			MLineCoef1Major = MLineCoef1 * MajorAxis;
			MLineCoef2 = (3.0 * ESq / 8.0) + (3.0 * e4 / 32.0) + (45.0 * e6 / 1024.0);
			MLineCoef3 = (15.0 * e4 / 256.0) + (45.0 * e6 / 1024.0);
			MLineCoef4 = (35.0 * e6 / 3072.0);
			MOrigin = MajorAxis * (
				(MLineCoef1 * naturalOrigin.Latitude)
				- (MLineCoef2 * Math.Sin(2.0 * naturalOrigin.Latitude))
				+ (MLineCoef3 * Math.Sin(4.0 * naturalOrigin.Latitude))
				- (MLineCoef4 * Math.Sin(6.0 * naturalOrigin.Latitude))
			);
			MOriginYOffset = MOrigin - falseProjectedOffset.Y;
			OneMinusESq = (1.0 - ESq);
			ESecSq = spheroid.ESecondSquared;
			OneMinusESqSqrt = Math.Sqrt(OneMinusESq);
			double ep = (1.0 - OneMinusESqSqrt) / (1.0 + OneMinusESqSqrt);
			double ep2 = ep * ep;
			double ep3 = ep2 * ep;
			double ep4 = ep3 * ep;
			LatLineCoef1 = (3.0 * ep / 2.0) - (27.0 * ep3 / 32.0);
			LatLineCoef2 = (21.0 * ep2 / 16.0) - (55.0 * ep4 / 32.0);
			LatLineCoef3 = (151.0 * ep3 / 96.0)/* - (1097.0 * ep4 / 512.0)*/;
			LatLineCoef4 = (1097 * ep4 / 512);
		}

		public override Point2 TransformValue(GeographicCoord coord) {
			double v = Math.Sin(coord.Latitude);
			v = MajorAxis / Math.Sqrt(1.0 - (ESq * v * v));
			double c = Math.Cos(coord.Latitude);
			double a = (coord.Longitude - NaturalOrigin.Longitude) * c;
			c = ESecSq * c * c;
			double tanLat = Math.Tan(coord.Latitude);
			double t = tanLat * tanLat;
			double a2 = a * a;

			double east = FalseProjectedOffset.X + (
				v * ( a - (((t * a * a2) * (20.0 + (a2 * (8.0 + (8.0 * c) - t)))) / 120.0))
			);
			double m = MajorAxis * (
				(MLineCoef1 * coord.Latitude)
				- (MLineCoef2 * Math.Sin(2.0 * coord.Latitude))
				+ (MLineCoef3 * Math.Sin(4.0 * coord.Latitude))
				- (MLineCoef4 * Math.Sin(6.0 * coord.Latitude))
			);
			double x =
				m
				- MOrigin
				+ ((v * tanLat * a2 * (12 + (a2 * (5.0 - t + (6.0 * c))))) / 24.0)
			;
			double north = FalseProjectedOffset.Y + x;

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
							(MLineCoef1 * coord.Lat)
							- (MLineCoef2 * System.Math.Sin(2.0 * coord.Lat))
							+ (MLineCoef3 * System.Math.Sin(4.0 * coord.Lat))
							- (MLineCoef4 * System.Math.Sin(6.0 * coord.Lat))
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

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get {
				return 0 != MLineCoef1Major
					&& 0 != MajorAxis
					&& 0 != OneMinusESqSqrt;
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
