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
		protected readonly double OneMinusESquared;
		protected readonly double Ak;
		protected readonly double Bk;
		protected readonly double Line1Coefficient;
		protected readonly double Line2Coefficient;
		protected readonly double Line3Coefficient;
		protected readonly double Line4Coefficient;
		protected readonly double Line1CoefficientScaled;
		protected readonly double Line2CoefficientScaled;
		protected readonly double Line3CoefficientScaled;
		protected readonly double Line4CoefficientScaled;

		private class Inverted : InvertedTransformationBase<TransverseMercator,Point2,GeographicCoordinate>
		{

			private readonly double _majorAxisScaleFactor;

			public Inverted(TransverseMercator core) : base(core) {
				_majorAxisScaleFactor = Core.MajorAxis * Core.ScaleFactor;
			}

			public override GeographicCoordinate TransformValue(Point2 coordinate) {
				var lat = (
					(coordinate.Y - Core.FalseProjectedOffset.Y)
					/ _majorAxisScaleFactor
				) + Core.NaturalOrigin.Latitude;
				double m = 0.0;
				for(int i =0; i < 20; i++){
					var w = lat - Core.NaturalOrigin.Latitude;
					var nw = lat + Core.NaturalOrigin.Latitude;
					var mOld = m;

					m = (
						(w * Core.Line1Coefficient)
						- (Math.Sin(w) * Math.Cos(nw) * Core.Line2Coefficient)
						+ (Math.Sin(w * 2) * Math.Cos(nw * 2) * Core.Line3Coefficient)
						- (Math.Sin(w * 3) * Math.Sin(nw * 3) * Core.Line4Coefficient)
					) * Core.Bk;

// ReSharper disable CompareOfFloatsByEqualityOperator
					if (mOld == m || Math.Abs(coordinate.Y - Core.FalseProjectedOffset.Y - m) < .00001) break;
// ReSharper restore CompareOfFloatsByEqualityOperator

					lat = ((coordinate.Y - Core.FalseProjectedOffset.Y - m)/_majorAxisScaleFactor) + lat;
				}
				var sinLatSquared = Math.Sin(lat);
				sinLatSquared = sinLatSquared * sinLatSquared;

				var p = 1.0 - (Core.ESq * sinLatSquared); // used here as oneMinusESquaredAndSinLatSquared
				
				var v = _majorAxisScaleFactor / Math.Sqrt(p);
				var v2 = v*v;
				var v4 = v2*v2;
				var v5 = v4*v;

				p = _majorAxisScaleFactor * (1.0 - Core.ESq) * Math.Pow(p, -1.5); // this is the true use of 'p'
				var pv = p*v;
				var etaSq = (v / p) - 1.0;
				var tanLat = Math.Tan(lat);
				var tanLat2 = tanLat*tanLat;
				var tanLat4 = tanLat2*tanLat2;
				var cosLat = Math.Cos(lat);
				var secLat = 1.0 / cosLat;

				var de = coordinate.X - Core.FalseProjectedOffset.X;
				var de2 = de * de;
				var de3 = de2 * de;
				var de4 = de3 * de;
				var de6 = de4 * de2;

				return new GeographicCoordinate(
					lat
					- (
						(tanLat / (pv * 2.0))
						* de2
					)
					+ (
						(tanLat / (pv * v2 * 24.0))
						* (
							(((etaSq * -9.0) + 3.0) * tanLat2)
							+ etaSq + 5
						)
						* de4
					)
					- (
						(tanLat / (pv * v4 * 720.0))
						* ((tanLat2 * 90.0) + (tanLat4 * 45.0) + 61.0)
						* de6
					)
					,
					Core.NaturalOrigin.Longitude
					+ (secLat / v * de)
					- (
						(secLat / (v2 * v * 6.0))
						* ((v / p) + (tanLat2 * 2.0))
						* de3
					)
					+ (
						(secLat / (v5 * 120.0))
						* ((tanLat2 * 28.0) + (tanLat4 * 24.0) + 5.0)
						* de4 * de
					)
					- (
						(
							+(tanLat2 * 662.0)
							+ (tanLat4 * 1320.0)
							+ (tanLat4 * tanLat2 * 720.0)
							+ 61.0
						)
						* (secLat / (v5 * v2 * 5040.0))
						* de6 * de
					)
				);
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
		) : base(naturalOrigin, falseProjectedOffset, spheroid)
		{
			ScaleFactor = scaleFactor;
			ESecSq58 = 58.0 * ESecSq;
			ESecSq330 = 330.0 * ESecSq;
			ESecSq8 = 8.0 * ESecSq;
			ESecSq9 = 9.0 * ESecSq;
			ESecSq252 = 252.0 * ESecSq;
			OneMinusESquared = 1.0 - ESq;
			Ak = MajorAxis*ScaleFactor;
			Bk = Spheroid.B*ScaleFactor;
			var n = (MajorAxis - Spheroid.B) / (MajorAxis + Spheroid.B);
			var n2 = n * n;
			var n3 = n2 * n;

			Line1Coefficient = (5.0 / 4.0 * n2) + (5.0 / 4.0 * n3) + n + 1.0;
			Line2Coefficient = ((n + n2) * 3.0) + (21 / 8.0 * n3);
			Line3Coefficient = (15.0 / 8.0 * n2) + (15.0 / 8.0 * n3);
			Line4Coefficient = 35.0 / 24.0 * n3;
			Line1CoefficientScaled = Line1Coefficient * Bk;
			Line2CoefficientScaled = Line2Coefficient * Bk;
			Line3CoefficientScaled = Line3Coefficient * Bk;
			Line4CoefficientScaled = Line4Coefficient * Bk;
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return 0 != ScaleFactor && base.HasInverse; }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public override Point2 TransformValue(GeographicCoordinate coordinate) {
			var eastCoefficient = Math.Cos(coordinate.Latitude); // act as cosLat for a bit
			var cosLat2 = eastCoefficient * eastCoefficient;
			var sinCosLat = Math.Sin(coordinate.Latitude);
			var w = 1.0 - (sinCosLat * sinCosLat * ESq);
			var v = Ak / Math.Sqrt(w);
			var vp = Math.Sqrt(w) / OneMinusESquared;
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
			return new Point2(
				(
					(
						(
							(
								(
									(
										((tanLat2 - 18.0) * tanLat2)
										+ (((58.0 * tanLat2) - 14.0) * etaSq)
										+ 5
									) * cosLatDeltaLon2 / 20.0
								) + vp - tanLat2
							) * cosLatDeltaLon2 / 6.0
						) + 1
					) * eastCoefficient
				) + FalseProjectedOffset.X
				,
				(
					(
						(w * Line1CoefficientScaled)
						- (Math.Sin(w) * Math.Cos(nw) * Line2CoefficientScaled)
						+ (Math.Sin(w * 2.0) * Math.Cos(nw * 2.0) * Line3CoefficientScaled)
						- (Math.Sin(w * 3.0) * Math.Cos(nw * 3.0) * Line4CoefficientScaled)
					)
					+ (
						(
							(
								(
									(
										(
											(
												(tanLat2 - 58.0)
												* tanLat2
											) + 61
										) * cosLatDeltaLon2 / 30.0
									) + (9 * etaSq) - tanLat2 + 5
								) * cosLatDeltaLon2 / 12.0
							) + 1
						) * northCoefficient / 2.0
					)
					+ FalseProjectedOffset.Y
				)
			);
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
