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
	/// A transverse Mercator projection.
	/// </summary>
	public class TransverseMercator :
		CassiniSoldner,
		IEquatable<TransverseMercator>
	{

		protected readonly double ScaleFactor;
		protected readonly double ESecSq58;
		protected readonly double ESecSq330;
		protected readonly double ESecSq8;
		protected readonly double ESecSq9;
		protected readonly double ESecSq252;

		private class Inverted : InvertedTransformationBase<TransverseMercator,Point2,GeographicCoord>
		{

			private readonly double _majorAxisScaleFactor;

			public Inverted(TransverseMercator core)
				: base(core) {
				if (
					0 == Core.ScaleFactor
					|| 0 == Core.MLineCoef1Major
					|| 0 == Core.MajorAxis
					|| 0 == Core.OneMinusESqSqrt
				) {
					throw new ArgumentException("Core cannot be inverted.");
				}
				_majorAxisScaleFactor = Core.MajorAxis * Core.ScaleFactor;
			}

			public override GeographicCoord TransformValue(Point2 coord) {
				double n = (Core.MajorAxis - Core.Spheroid.B) / (Core.MajorAxis + Core.Spheroid.B);
				double n2 = n * n;
				double n3 = n2 * n;
				double lat = ((coord.Y - Core.FalseProjectedOffset.Y) / _majorAxisScaleFactor) +
							 Core.NaturalOrigin.Latitude;
				double m = 0;
				double oldm;
				do {
					double w = lat - Core.NaturalOrigin.Latitude;
					double nw = lat + Core.NaturalOrigin.Latitude;
					double sinW = Math.Sin(w);
					double cosNW = Math.Cos(nw);
					double sin2W = Math.Sin(w * 2);
					double cos2NW = Math.Cos(nw * 2);
					double sin3W = Math.Sin(w * 3);
					double cos3NW = Math.Sin(nw * 3);
					oldm = m;
					m = Core.Spheroid.B * Core.ScaleFactor * (
					((1 + n + (5 * n2 / 4.0) + (5 * n3 / 4.0)) * w)
						- (((3 * n) + (3 * n2) + (21 * n3 / 8.0)) * sinW * cosNW)
						+ (((15 * n2 / 8.0) + (15 * n3 / 8.0)) * sin2W * cos2NW)
						- ((35 * n3 / 24.0) * sin3W * cos3NW)
					);
					if (Math.Abs(coord.Y - Core.FalseProjectedOffset.Y - m) >= .00001 && oldm != m)
						lat = ((coord.Y - Core.FalseProjectedOffset.Y - m) / _majorAxisScaleFactor) + lat;
					else
						break;
				} while (true);
				double sinLat = Math.Sin(lat);
				double v = _majorAxisScaleFactor / Math.Sqrt(1.0 - (Core.ESq * sinLat * sinLat));
				double p = _majorAxisScaleFactor * (1.0 - Core.ESq) * Math.Pow(1.0 - (Core.ESq * sinLat * sinLat), -1.5);
				double etaSq = (v / p) - 1.0;
				double tanLat = Math.Tan(lat);
				double cosLat = Math.Cos(lat);
				double secLat = 1.0 / cosLat;
				double rom7 = tanLat / (2.0 * p * v);
				double rom8 = (tanLat / (24 * p * v * v * v)) * (5 + (3 * tanLat * tanLat) + etaSq - (9 * (tanLat * tanLat) * etaSq));
				double rom9 = (tanLat / (720 * p * v * v * v * v * v)) * (61 + (90 * tanLat * tanLat) + (45 * tanLat * tanLat * tanLat * tanLat));
				double rom10 = secLat / v;
				double rom11 = (secLat / (6 * v * v * v)) * ((v / p) + (2 * tanLat * tanLat));
				double rom12 = (secLat / (120 * v * v * v * v * v)) * (5 + (28 * tanLat * tanLat) + (24 * tanLat * tanLat * tanLat * tanLat));
				double rom12A = (secLat / (5040 * v * v * v * v * v * v * v)) *
								(61 + (662 * tanLat * tanLat) + (1320 * tanLat * tanLat * tanLat * tanLat) +
								 (720 * tanLat * tanLat * tanLat * tanLat * tanLat * tanLat));
				double de = coord.X - Core.FalseProjectedOffset.X;
				double de2 = de * de;
				double de3 = de2 * de;
				double de4 = de2 * de2;
				double de5 = de4 * de;
				double de6 = de3 * de3;
				double de7 = de6 * de;

				return new GeographicCoord(
					lat
					- (rom7 * de2)
					+ (rom8 * de4)
					- (rom9 * de6)
					,
					Core.NaturalOrigin.Longitude
					+ (rom10 * de)
					- (rom11 * de3)
					+ (rom12 * de5)
					- (rom12A * de7)
				);

				/*double latp = (Core.MOrigin + ((coord.Y - Core.FalseProjectedOffset.Y) / Core.ScaleFactor)) / Core.MLineCoef1Major;
				latp = (
					latp
					+ (Core.LatLineCoef1 * System.Math.Sin(2.0 * latp))
					+ (Core.LatLineCoef2 * System.Math.Sin(4.0 * latp))
					+ (Core.LatLineCoef3 * System.Math.Sin(6.0 * latp))
					+ (Core.LatLineCoef4 * System.Math.Sin(8.0 * latp))
				);
				double lesq = System.Math.Sin(latp); lesq = 1.0 - (lesq * lesq * Core.ESq);
				double d = ((coord.X - Core.FalseProjectedOffset.X) * System.Math.Sqrt(lesq)) / _majorAxisScaleFactor;
				double d2 = d * d;
				double cosLatp = System.Math.Cos(latp);
				double c = Core.ESecSq * cosLatp * cosLatp;
				double t = System.Math.Tan(latp);
				lesq *= t;
				t = t * t;
				return new LatLon(
					latp - (
						((lesq) / Core.OneMinusESqSqrt)
						* d2
						* (
							0.5
							+ (
								d2 * (
									(
										(
											61.0
											+ (t * (90 + (45 * t)))
											+ (c * (298 - (3.0 * c)))
											- Core.ESecSq252
										)
										* d2
										/ 720.0
									)
									- (
										(
											5.0
											+ (3 * t)
											+ (c * (10 - (4 * c)))
											- Core.ESecSq9
										)
										/ 24.0
									)
								)
							)
						)
					),
					Core.NaturalOrigin.Lon + (
						(
							d
							+ (d2 * d * (
								(
									(
										5.0
										- (c * (2.0 + (3.0 * c)))
										+ (t * (28.0 + (24.0 * t)))
										+ Core.ESecSq8
									)
									* d2
									/ 120.0
								)
								- (
									(1.0 + (2.0 * t) + c)
									/ 6.0
								)
							))
						)
						/ cosLatp
					)
				);*/
			}

		}

		/// <summary>
		/// Constructs a new transverse mercator projection.
		/// </summary>
		/// <param name="naturalOrigin">The natural origin.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="scaleFactor">The scale factor.</param>
		/// <param name="spheroid">The spheroid.</param>
		public TransverseMercator(
			GeographicCoord naturalOrigin,
			Vector2 falseProjectedOffset,
			double scaleFactor,
			ISpheroid<double> spheroid
		)
			: base(naturalOrigin, falseProjectedOffset, spheroid)
		{
			ScaleFactor = scaleFactor;
			ESecSq58 = 58.0 * ESecSq;
			ESecSq330 = 330.0 * ESecSq;
			ESecSq8 = 8.0 * ESecSq;
			ESecSq9 = 9.0 * ESecSq;
			ESecSq252 = 252.0 * ESecSq;
		}

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { return 0 != ScaleFactor && base.HasInverse; }
		}

		[Obsolete]
		private static double Arctanh(double x) {
			return Math.Log((1 + x) / (1 - x)) / 2.0;
		}

		public override Point2 TransformValue(GeographicCoord coord) {

			double ak = MajorAxis * ScaleFactor;
			double bk = Spheroid.B * ScaleFactor;
			double n = (MajorAxis - Spheroid.B) / (MajorAxis + Spheroid.B);
			double n2 = n * n;
			double n3 = n2 * n;
			double mLine1Coef = bk * (1 + n + (5.0 * n2 / 4.0) + (5.0 * n3 / 4.0));
			double mLine2Coef = bk * ((3 * (n + n2)) + (21 * n3 / 8.0));
			double mLine3Coef = bk * ((15.0 * n2 / 8.0) + (15.0 * n3 / 8.0));
			double mLine4Coef = bk * (35.0 * n3 / 24.0);
			double nESq = 1.0 - ESq;

			double eastCoef = Math.Cos(coord.Latitude); // act as cosLat for a bit
			double cosLat2 = eastCoef * eastCoef;
			double sinCosLat = Math.Sin(coord.Latitude);
			double w = 1.0 - (ESq * sinCosLat * sinCosLat);
			double v = ak / Math.Sqrt(w);
			double vp = Math.Sqrt(w) / nESq;
			sinCosLat *= eastCoef; // sinCosLat was recycled
			double tanLat2 = Math.Tan(coord.Latitude);
			tanLat2 *= tanLat2; // tanLat is not used anymore so just double it up on its own
			w = coord.Latitude - NaturalOrigin.Latitude; // w was recycled
			double nw = coord.Latitude + NaturalOrigin.Latitude;
			double etaSq = coord.Longitude - NaturalOrigin.Longitude; // act as deltaLon for a bit
			double northCoef = etaSq * etaSq; // act as deltaLon2
			double cosLatDeltaLon2 = cosLat2 * northCoef;
			northCoef *= v * sinCosLat; // recycled from deltaLon, as as northCoef
			eastCoef *= v * etaSq; // recycled cosLat
			etaSq = vp - 1.0; // recycled deltaLon
			double north = (
				(
					(mLine1Coef * w)
					- (mLine2Coef * Math.Sin(w) * Math.Cos(nw))
					+ (mLine3Coef * Math.Sin(w * 2.0) * Math.Cos(nw * 2.0))
					- (mLine4Coef * Math.Sin(w * 3.0) * Math.Cos(nw * 3.0))
				)
				+ FalseProjectedOffset.Y
				+ (
					northCoef / 2.0
					* (
						1
						+ (
							cosLatDeltaLon2 / 12.0
							* (
								5
								+ (9 * etaSq)
								- tanLat2
								+ (
									cosLatDeltaLon2 / 30.0
									* (
										61
										+ tanLat2 * (tanLat2 - 58.0)
									)
								)
							)
						)
					)
				)
			);
			double east = FalseProjectedOffset.X + (
				eastCoef * (
					1
					+ (
						cosLatDeltaLon2 / 6.0
						* (
							vp
							- tanLat2
							+ (
								cosLatDeltaLon2 / 20.0
								* (
									5
									+ (tanLat2 * (tanLat2 - 18.0))
									+ (etaSq * (14.0 - (58.0 * tanLat2)))
								)
							)
						)
					)
				)
			);
			return new Point2(east, north);
		}

		public override string Name {
			get { return "Transverse Mercator"; }
		}

		public override IEnumerable<INamedParameter> GetParameters() {
			return base.GetParameters().Concat(
				new INamedParameter[] {
                    new NamedParameter<double>(NamedParameter.NameScaleFactorAtNaturalOrigin,ScaleFactor)
                }
			);
		}


		public bool Equals(TransverseMercator other) {
			return !ReferenceEquals(other, null)
				&& (
					NaturalOrigin == other.NaturalOrigin
					&& ScaleFactor == other.ScaleFactor
					&& FalseProjectedOffset.Equals(other.FalseProjectedOffset)
					&& Spheroid.Equals(other.Spheroid)
				)
			;
		}

		public override bool Equals(object obj) {
			return null != obj
				&& (ReferenceEquals(this, obj) || Equals(obj as TransverseMercator));
		}

		public override int GetHashCode() {
			return FalseProjectedOffset.GetHashCode() ^ ScaleFactor.GetHashCode();
		}

	}
}
