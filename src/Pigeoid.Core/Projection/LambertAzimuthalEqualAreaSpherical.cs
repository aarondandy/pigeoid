// TODO: source header

using System;
using Pigeoid.Interop;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class LambertAzimuthalEqualAreaSpherical :
		ProjectionBase,
		IEquatable<LambertAzimuthalEqualAreaSpherical>
	{

		private class Inverted : InvertedTransformationBase<LambertAzimuthalEqualAreaSpherical,Point2,GeographicCoord>
		{

			private readonly double _r2;

			public Inverted(LambertAzimuthalEqualAreaSpherical core)
				: base(core)
			{
				_r2 = core.R * 2.0;
			}

			public override GeographicCoord TransformValue(Point2 source) {
				double p = Math.Sqrt((source.X * source.X) + (source.Y * source.Y));
				if (0 == p)
					return Core.GeographicOrigin;

				double c = 2.0 * Math.Asin(p / (_r2));
				double cosC = Math.Cos(c);
				double sinC = Math.Sin(c);
				double lat = Math.Asin(
					(cosC * Core.SinLatOrg)
					+ ((source.Y * sinC * Core.CosLatOrg) / p)
				);
				double lon;
				if (Core.GeographicOrigin.Latitude == HalfPi)
					lon = Core.GeographicOrigin.Longitude + Math.Atan2(source.X, -source.Y);
				else if (Core.GeographicOrigin.Latitude == -HalfPi)
					lon = Core.GeographicOrigin.Longitude + Math.Atan2(source.X, source.Y);
				else
					lon = Core.GeographicOrigin.Longitude + Math.Atan2(
						source.X * sinC,
						(p * Core.CosLatOrg * cosC) - (source.Y * Core.SinLatOrg * sinC)
					);
				return new GeographicCoord(lat, lon);
			}
		}


		public readonly double R;
		public readonly GeographicCoord GeographicOrigin;
		private readonly double SinLatOrg;
		private readonly double CosLatOrg;

		public LambertAzimuthalEqualAreaSpherical(
			GeographicCoord geogOrigin,
			Vector2 falseOffset,
			ISpheroid<double> spheroid
		)
			: base(falseOffset, spheroid) {
			GeographicOrigin = geogOrigin;
			R = spheroid.A * Math.Sqrt(
				(1.0 - (((1.0 - ESq) / (2.0 * E)) * Math.Log((1.0 - E) / (1.0 + E))))
				/ 2.0
			);
			SinLatOrg = Math.Sin(geogOrigin.Latitude);
			CosLatOrg = Math.Cos(geogOrigin.Latitude);
		}


		public override Point2 TransformValue(GeographicCoord source) {
			double deltaLon = source.Longitude - GeographicOrigin.Longitude;
			double cosLat = Math.Cos(source.Latitude);
			double cosDeltaLonCosLat = Math.Cos(deltaLon) * cosLat;
			double sinLat = Math.Sin(source.Latitude);
			double rk = R * Math.Sqrt(
				2.0 /
				(1.0 + (SinLatOrg * sinLat) + (CosLatOrg * cosDeltaLonCosLat))
			);
			double x = rk * cosLat * Math.Sin(deltaLon);
			double y = rk * ((CosLatOrg * sinLat) - (SinLatOrg * cosDeltaLonCosLat));
			return new Point2(x, y);
		}

		public override bool HasInverse {
			get { return true; }
		}

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override string Name {
			get { return CoordinateOperationStandardNames.LambertAzimuthalEqualArea; }
		}

		public bool Equals(LambertAzimuthalEqualAreaSpherical other) {
			return !ReferenceEquals(other, null)
				&& (
					GeographicOrigin.Equals(other.GeographicOrigin)
					&& FalseProjectedOffset.Equals(other.FalseProjectedOffset)
					&& Spheroid.Equals(other.Spheroid)
				)
			;
		}

		public override bool Equals(object obj) {
			return null != obj
				&& (ReferenceEquals(this, obj) || Equals(obj as LambertAzimuthalEqualAreaSpherical));
		}

		public override int GetHashCode() {
			return -GeographicOrigin.GetHashCode() ^ FalseProjectedOffset.GetHashCode();
		}

	}
}
