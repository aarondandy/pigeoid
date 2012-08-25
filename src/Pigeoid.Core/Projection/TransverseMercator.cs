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

		private class Inverted : InvertedTransformationBase<TransverseMercator,Point2,GeographicCoordinate>
		{

			private readonly double _majorAxisScaleFactor;

			public Inverted(TransverseMercator core)
				: base(core) {
				if (!core.HasInverse)
					throw new ArgumentException("Core cannot be inverted.");
				
				_majorAxisScaleFactor = Core.MajorAxis * Core.ScaleFactor;
			}

			public override GeographicCoordinate TransformValue(Point2 coordinate) {
				var n = (Core.MajorAxis - Core.Spheroid.B) / (Core.MajorAxis + Core.Spheroid.B);
				var n2 = n * n;
				var n3 = n2 * n;
				var lat = ((coordinate.Y - Core.FalseProjectedOffset.Y) / _majorAxisScaleFactor) +
							 Core.NaturalOrigin.Latitude;
				double m = 0;
				do {
					var w = lat - Core.NaturalOrigin.Latitude;
					var nw = lat + Core.NaturalOrigin.Latitude;
					var sinW = Math.Sin(w);
					var cosNw = Math.Cos(nw);
					var sin2W = Math.Sin(w * 2);
					var cos2Nw = Math.Cos(nw * 2);
					var sin3W = Math.Sin(w * 3);
					var cos3Nw = Math.Sin(nw * 3);
					var mOld = m;
					m = Core.Spheroid.B * Core.ScaleFactor * (
					((1 + n + (5 * n2 / 4.0) + (5 * n3 / 4.0)) * w)
						- (((3 * n) + (3 * n2) + (21 * n3 / 8.0)) * sinW * cosNw)
						+ (((15 * n2 / 8.0) + (15 * n3 / 8.0)) * sin2W * cos2Nw)
						- ((35 * n3 / 24.0) * sin3W * cos3Nw)
					);

// ReSharper disable CompareOfFloatsByEqualityOperator
					if (!(Math.Abs(coordinate.Y - Core.FalseProjectedOffset.Y - m) >= .00001) || mOld == m) break;
// ReSharper restore CompareOfFloatsByEqualityOperator

					lat = ((coordinate.Y - Core.FalseProjectedOffset.Y - m)/_majorAxisScaleFactor) + lat;
				} while (true);
				var sinLat = Math.Sin(lat);
				var v = _majorAxisScaleFactor / Math.Sqrt(1.0 - (Core.ESq * sinLat * sinLat));
				var p = _majorAxisScaleFactor * (1.0 - Core.ESq) * Math.Pow(1.0 - (Core.ESq * sinLat * sinLat), -1.5);
				var etaSq = (v / p) - 1.0;
				var tanLat = Math.Tan(lat);
				var cosLat = Math.Cos(lat);
				var secLat = 1.0 / cosLat;
				var rom7 = tanLat / (2.0 * p * v);
				var rom8 = (tanLat / (24 * p * v * v * v)) * (5 + (3 * tanLat * tanLat) + etaSq - (9 * (tanLat * tanLat) * etaSq));
				var rom9 = (tanLat / (720 * p * v * v * v * v * v)) * (61 + (90 * tanLat * tanLat) + (45 * tanLat * tanLat * tanLat * tanLat));
				var rom10 = secLat / v;
				var rom11 = (secLat / (6 * v * v * v)) * ((v / p) + (2 * tanLat * tanLat));
				var rom12 = (secLat / (120 * v * v * v * v * v)) * (5 + (28 * tanLat * tanLat) + (24 * tanLat * tanLat * tanLat * tanLat));
				var rom12A = (secLat / (5040 * v * v * v * v * v * v * v)) *
								(61 + (662 * tanLat * tanLat) + (1320 * tanLat * tanLat * tanLat * tanLat) +
								 (720 * tanLat * tanLat * tanLat * tanLat * tanLat * tanLat));
				var de = coordinate.X - Core.FalseProjectedOffset.X;
				var de2 = de * de;
				var de3 = de2 * de;
				var de4 = de2 * de2;
				var de5 = de4 * de;
				var de6 = de3 * de3;
				var de7 = de6 * de;

				return new GeographicCoordinate(
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

				/*double latp = (Core.MOrigin + ((coordinate.Y - Core.FalseProjectedOffset.Y) / Core.ScaleFactor)) / Core.MLineCoefficient1Major;
				latp = (
					latp
					+ (Core.LatLineCoefficient1 * System.Math.Sin(2.0 * latp))
					+ (Core.LatLineCoefficient2 * System.Math.Sin(4.0 * latp))
					+ (Core.LatLineCoefficient3 * System.Math.Sin(6.0 * latp))
					+ (Core.LatLineCoefficient4 * System.Math.Sin(8.0 * latp))
				);
				double lesq = System.Math.Sin(latp); lesq = 1.0 - (lesq * lesq * Core.ESq);
				double d = ((coordinate.X - Core.FalseProjectedOffset.X) * System.Math.Sqrt(lesq)) / _majorAxisScaleFactor;
				double d2 = d * d;
				double cosLatp = System.Math.Cos(latp);
				double c = Core.ESecSq * cosLatp * cosLatp;
				double t = System.Math.Tan(latp);
				lesq *= t;
				t = t * t;
				return new LatLon(
					latp - (
						((lesq) / Core.SquareRootOfOneMinusESq)
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
		/// Constructs a new Transverse Mercator projection.
		/// </summary>
		/// <param name="naturalOrigin">The natural origin.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="scaleFactor">The scale factor.</param>
		/// <param name="spheroid">The spheroid.</param>
		public TransverseMercator(
			GeographicCoordinate naturalOrigin,
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

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return 0 != ScaleFactor && base.HasInverse; }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		[Obsolete]
		private static double ArcTanH(double x) {
			return Math.Log((1 + x) / (1 - x)) / 2.0;
		}

		public override Point2 TransformValue(GeographicCoordinate coordinate) {

			var ak = MajorAxis * ScaleFactor;
			var bk = Spheroid.B * ScaleFactor;
			var n = (MajorAxis - Spheroid.B) / (MajorAxis + Spheroid.B);
			var n2 = n * n;
			var n3 = n2 * n;
			var mLine1Coefficient = bk * (1 + n + (5.0 * n2 / 4.0) + (5.0 * n3 / 4.0));
			var mLine2Coefficient = bk * ((3 * (n + n2)) + (21 * n3 / 8.0));
			var mLine3Coefficient = bk * ((15.0 * n2 / 8.0) + (15.0 * n3 / 8.0));
			var mLine4Coefficient = bk * (35.0 * n3 / 24.0);
			var nESq = 1.0 - ESq;
			var eastCoefficient = Math.Cos(coordinate.Latitude); // act as cosLat for a bit
			var cosLat2 = eastCoefficient * eastCoefficient;
			var sinCosLat = Math.Sin(coordinate.Latitude);
			var w = 1.0 - (ESq * sinCosLat * sinCosLat);
			var v = ak / Math.Sqrt(w);
			var vp = Math.Sqrt(w) / nESq;
			sinCosLat *= eastCoefficient; // sinCosLat was recycled
			var tanLat2 = Math.Tan(coordinate.Latitude);
			tanLat2 *= tanLat2; // tanLat is not used anymore so just double it up on its own
			w = coordinate.Latitude - NaturalOrigin.Latitude; // w was recycled
			var nw = coordinate.Latitude + NaturalOrigin.Latitude;
			var etaSq = coordinate.Longitude - NaturalOrigin.Longitude; // act as deltaLon for a bit
			var northCoefficient = etaSq * etaSq; // act as deltaLon2
			var cosLatDeltaLon2 = cosLat2 * northCoefficient;
			northCoefficient *= v * sinCosLat; // recycled from deltaLon, as northCoef
			eastCoefficient *= v * etaSq; // recycled cosLat
			etaSq = vp - 1.0; // recycled deltaLon
			var north = (
				(
					(mLine1Coefficient * w)
					- (mLine2Coefficient * Math.Sin(w) * Math.Cos(nw))
					+ (mLine3Coefficient * Math.Sin(w * 2.0) * Math.Cos(nw * 2.0))
					- (mLine4Coefficient * Math.Sin(w * 3.0) * Math.Cos(nw * 3.0))
				)
				+ FalseProjectedOffset.Y
				+ (
					northCoefficient / 2.0
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
			var east = FalseProjectedOffset.X + (
				eastCoefficient * (
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
// ReSharper disable CompareOfFloatsByEqualityOperator
					&& ScaleFactor == other.ScaleFactor
// ReSharper restore CompareOfFloatsByEqualityOperator
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
