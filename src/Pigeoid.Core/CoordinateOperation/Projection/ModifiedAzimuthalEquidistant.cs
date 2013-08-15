using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class ModifiedAzimuthalEquidistant : ProjectionBase
    {

        private class Inverse : InvertedTransformationBase<ModifiedAzimuthalEquidistant, Point2, GeographicCoordinate>
        {

            public Inverse(ModifiedAzimuthalEquidistant core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                var dx = value.X - Core.FalseProjectedOffset.X;
                var dy = value.Y - Core.FalseProjectedOffset.Y;
                var c = Math.Sqrt((dx * dx) + (dy * dy));
                var alpha = Math.Atan(dx / dy);
                var cosAlpha = Math.Cos(alpha);
                var a = -Core.ESq * Core.CosLatOriginSq * cosAlpha * cosAlpha / (1 - Core.ESq);
                var b = 3.0 * Core.ESq * (1 - a) * Core.SinLatOrigin * Core.CosLatOrigin * cosAlpha / (1 - Core.ESq);
                var d = c / Core.VOrigin;
                var j = d
                    - (a * (1 + a) * d * d * d / 6.0)
                    - (b * (1 + (3.0 * a)) * d * d * d * d / 24.0);
                var k = 1 - (a * j * j / 2.0) - (b * j * j * j / 6.0);
                var psi = Math.Asin((Core.SinLatOrigin * Math.Cos(j)) + (Core.CosLatOrigin * Math.Sin(j) * cosAlpha));
                var lat = Math.Atan((1 - (Core.ESq * k * Core.SinLatOrigin / Math.Sin(psi))) * Math.Tan(psi) / (1 - Core.ESq));
                var lon = Core.GeographicOrigin.Longitude + Math.Asin(Math.Sin(alpha) * Math.Sin(j) / Math.Cos(psi));
                return new GeographicCoordinate(lat, lon);
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double VOrigin;
        protected readonly double SinLatOrigin;
        protected readonly double CosLatOrigin;
        protected readonly double CosLatOriginSq;

        public ModifiedAzimuthalEquidistant(GeographicCoordinate origin, Vector2 offset, ISpheroid<double> spheroid)
            : base(offset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = origin;
            SinLatOrigin = Math.Sin(origin.Latitude);
            CosLatOrigin = Math.Cos(origin.Latitude);
            CosLatOriginSq = CosLatOrigin * CosLatOrigin;
            VOrigin = MajorAxis / Math.Sqrt(1 - (ESq * SinLatOrigin * SinLatOrigin));
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var deltaLon = source.Longitude - GeographicOrigin.Longitude;
            var sinLat = Math.Sin(source.Latitude);
            var cosLat = Math.Cos(source.Latitude);
            var v = MajorAxis / Math.Sqrt(1 - (ESq * sinLat * sinLat));
            var psi = Math.Atan(((1 - ESq) * Math.Tan(source.Latitude)) + (ESq * VOrigin * SinLatOrigin / (v * cosLat)));
            var alpha = Math.Atan(Math.Sin(deltaLon) / ((CosLatOrigin * Math.Tan(psi)) - (SinLatOrigin * Math.Cos(deltaLon))));
            var g = E * SinLatOrigin / Math.Sqrt(1 - ESq);
            var h = E * CosLatOrigin * Math.Cos(alpha) / Math.Sqrt(1 - ESq);
            var sinAlpha = Math.Sin(alpha);
            var s = 0 == sinAlpha
                ? Math.Asin((CosLatOrigin * Math.Sin(psi)) - (SinLatOrigin * Math.Cos(psi)))
                : Math.Asin(Math.Sin(deltaLon) * Math.Cos(psi) / sinAlpha);
            var h2 = h * h;
            var s2 = s * s;
            var s3 = s2 * s;
            var s4 = s3 * s;
            var s5 = s4 * s;
            var g2 = g * g;
            var c = VOrigin * s * (
                (1 - (s2 * h2 * (1 - h2) / 6.0))
                + ((s3 / 8.0) * g * h * (1 - (2 * h2)))
                + ((s4 / 120.0) * ((h2 * (4.0 - (7.0 * h2))) - (3.0 * g2 * (1 - (7.0 * h2)))))
                - ((s5 / 48.0) * g * h)
            );
            return new Point2(
                FalseProjectedOffset.X + (c * sinAlpha),
                FalseProjectedOffset.Y + (c * Math.Cos(alpha))
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            [Pure] get { return 0 != VOrigin && !Double.IsNaN(VOrigin); }
        }
    }
}
