using System;
using System.Diagnostics.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
    public class Guam : ProjectionBase
    {

        private class Inverse : InvertedTransformationBase<Guam, Point2, GeographicCoordinate>
        {

            protected double Ep1;
            protected double Ep2;
            protected double Ep3;
            protected double Ep4;

            private double CalculateLat(double u) {
                return u
                    + (Math.Sin(2.0 * u) * ((Ep1 / 2.0) - (Ep3 * 27.0 / 32.0)))
                    + (Math.Sin(4.0 * u) * ((Ep2 / 16.0) - (Ep4 * 55.0 / 32.0)))
                    + (Math.Sin(6.0 * u) * ((Ep3 * 151.0 / 96.0)))
                    + (Math.Sin(8.0 * u) * ((Ep4 * 1097.0 / 512.0)))
                ;
            }

            public Inverse(Guam core)
                : base(core) {
                Contract.Requires(core != null);
                Ep1 = (1 - Math.Sqrt(1 - Core.ESq)) / (1 + Math.Sqrt(1 - Core.ESq));
                Ep2 = Ep1 * Ep1;
                Ep3 = Ep2 * Ep1;
                Ep4 = Ep3 * Ep1;
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                var dx = value.X - Core.FalseProjectedOffset.X;
                var dxSq = dx * dx;
                var dy = value.Y - Core.FalseProjectedOffset.Y;
                var lat = Core.GeographicOrigin.Latitude;
                var sinLat = Math.Sin(lat);
                for (int i = 0; i < 3; i++) {
                    var m = Core.MOrigin + dy - (dxSq * Math.Tan(lat) * Math.Sqrt(1 - (Core.ESq * sinLat * sinLat)) / (2.0 * Core.MajorAxis));
                    var u = m / (Core.MajorAxis * Core.MConstant1);
                    lat = CalculateLat(u);
                    sinLat = Math.Sin(lat);
                }
                var lon = Core.GeographicOrigin.Longitude + (dx * Math.Sqrt(1 - (Core.ESq * sinLat * sinLat)) / (Core.MajorAxis * Math.Cos(lat)));
                return new GeographicCoordinate(lat, lon);
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double MOrigin;
        protected readonly double SinLatOrigin;
        protected readonly double SinLatOriginSquared;
        protected readonly double E4;
        protected readonly double E6;
        protected readonly double MConstant1;

        private double CalculateM(double latitude) {
            return MajorAxis * (
                (MConstant1 * latitude)
                - (((ESq * 3.0 / 8.0) + (E4 * 3.0 / 32.0) + (E6 * 45.0 / 1024.0)) * Math.Sin(2.0 * latitude))
                + (((E4 * 15.0 / 256.0) + (E6 * 45.0 / 1024.0)) * Math.Sin(4.0 * latitude))
                - ((E6 * 35.0 / 3072.0) * Math.Sin(6.0 * latitude))
            );
        }

        public Guam(GeographicCoordinate origin, Vector2 offset, ISpheroid<double> spheroid) : base(offset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = origin;
            E4 = ESq * ESq;
            E6 = E4 * ESq;
            MConstant1 = (1 - (ESq / 4.0) - (E4 * 3.0 / 64.0) - (E6 * 5.0 / 256.0));
            SinLatOrigin = Math.Sin(origin.Latitude);
            SinLatOriginSquared = SinLatOrigin * SinLatOrigin;
            MOrigin = CalculateM(origin.Latitude);
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var sinLat = Math.Sin(source.Latitude);
            var x = MajorAxis * (source.Longitude - GeographicOrigin.Longitude) * Math.Cos(source.Latitude)
                / Math.Sqrt(1 - (ESq * sinLat * sinLat));
            var m = CalculateM(source.Latitude);
            var y = m - MOrigin + (x * x * Math.Tan(source.Latitude) * Math.Sqrt(1 - (ESq * sinLat * sinLat)) / (2.0 * MajorAxis));
            return new Point2(
                FalseProjectedOffset.X + x,
                FalseProjectedOffset.Y + y
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new InvalidOperationException("No inverse.");
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            [Pure] get { return 0 != MajorAxis; }
        }
    }
}
