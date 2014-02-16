using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class Bonne : SpheroidProjectionBase
    {

        private class Inverse : InvertedTransformationBase<Bonne, Point2, GeographicCoordinate>
        {

            public Inverse(Bonne core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                var dx = value.X - Core.FalseProjectedOffset.X;
                var dy = value.Y - Core.FalseProjectedOffset.Y;
                var adjustedY = (Core.MajorAxis * Core.MScalarOrigin / Core.SinLatOrigin) - dy;
                var p = Math.Sqrt((dx * dx) + (adjustedY * adjustedY));
                if (Core.GeographicOrigin.Latitude < 0) {
                    p = -p;
                }
                var m = (Core.MajorAxis * Core.MScalarOrigin / Core.SinLatOrigin) + Core.MOrigin - p;
                var u = m / (Core.MajorAxis * Core.MCalculationConstant1);
                var e = (1 - Math.Sqrt(1 - Core.ESq)) / (1 + Math.Sqrt(1 - Core.ESq));
                var e2 = e * e;
                var e3 = e2 * e;
                var e4 = e3 * e;
                var lat = u
                    + (((e * 3.0 / 2.0) - (e3 * 27.0 / 32.0)) * Math.Sin(2.0 * u))
                    + (((e2 * 21.0 / 16.0) - (e4 * 55.0 / 32.0)) * Math.Sin(4.0 * u))
                    + ((e3 * 151.0 / 96.0) * Math.Sin(6.0 * u))
                    + ((e4 * 1097.0 / 512.0) * Math.Sin(8.0 * u));
                var sinLat = Math.Sin(lat);
                var mScalar = Math.Cos(lat) / Math.Sqrt(1 - (Core.ESq * sinLat * sinLat));

                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (mScalar == 0)
                    return new GeographicCoordinate(lat, Core.GeographicOrigin.Longitude);
                // ReSharper restore CompareOfFloatsByEqualityOperator


                var lonCore = Core.GeographicOrigin.Longitude < 0
                    ? dx / ((Core.MajorAxis * Core.MScalarOrigin / Core.SinLatOrigin) - dy)
                    : dx / ((Core.MajorAxis * Core.MScalarOrigin / Core.SinLatOrigin) + dy);
                var lon = Core.GeographicOrigin.Longitude + (p * Math.Atan(lonCore) / (Core.MajorAxis * mScalar));
                return new GeographicCoordinate(lat, lon);
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double MOrigin;
        protected readonly double MScalarOrigin;
        protected readonly double E4;
        protected readonly double E6;
        protected readonly double SinLatOrigin;
        protected readonly double MCalculationConstant1;

        private double CalculateM(double latitude) {
            return MajorAxis * (
                (MCalculationConstant1 * latitude)
                - (((ESq * 3.0 / 8.0) + (E4 * 3.0 / 32.0) + (E6 * 45.0 / 1024.0)) * Math.Sin(2.0 * latitude))
                + (((E4 * 15.0 / 256.0) + (E6 * 45.0 / 1024.0)) * Math.Sin(4.0 * latitude))
                - ((E6 * 35.0 / 3072.0) * Math.Sin(6.0 * latitude))
            );
        }

        public Bonne(GeographicCoordinate geographicOrigin, Vector2 offset, ISpheroid<double> spheroid) : base(offset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geographicOrigin;
            E4 = ESq * ESq;
            E6 = E4 * ESq;
            MCalculationConstant1 = (1 - (ESq / 4.0) - (E4 * 3.0 / 64.0) - (E6 * 5.0 / 256.0));
            MOrigin = CalculateM(geographicOrigin.Latitude);
            SinLatOrigin = Math.Sin(GeographicOrigin.Latitude);
            MScalarOrigin = Math.Cos(geographicOrigin.Latitude) / Math.Sqrt(ESq * SinLatOrigin * SinLatOrigin);
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var m = CalculateM(source.Latitude);
            var sinLat = Math.Sin(source.Latitude);
            var mScalar = Math.Cos(source.Latitude) / Math.Sqrt(ESq * sinLat * sinLat);
            var p = (MajorAxis * MScalarOrigin / SinLatOrigin) + MOrigin + m;
            var t = MajorAxis * mScalar * (source.Longitude - GeographicOrigin.Longitude) / p;
            return new Point2(
                (p * Math.Sin(t)) + FalseProjectedOffset.X,
                ((MajorAxis * MScalarOrigin / SinLatOrigin) - (p * Math.Cos(t))) + FalseProjectedOffset.Y
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get { return 0 != SinLatOrigin && 0 != MajorAxis; }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }
    }
}
