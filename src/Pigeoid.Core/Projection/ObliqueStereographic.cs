using System;
using System.Diagnostics.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
    public class ObliqueStereographic : ProjectionBase
    {

        private class Inverted : InvertedTransformationBase<ObliqueStereographic, Point2, GeographicCoordinate>
        {

            private readonly double _halfChiOrigin;
            private readonly double _rk4;

            public Inverted(ObliqueStereographic core) : base(core) {
                Contract.Requires(core != null);
                _halfChiOrigin = Core.ChiOrigin / 2.0;
                _rk4 = Core.Rk2 * 2.0;
            }

            public override GeographicCoordinate TransformValue(Point2 source) {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                var dy = source.Y - Core.FalseProjectedOffset.Y;
                var dx = source.X - Core.FalseProjectedOffset.X;
                var g = Core.Rk2 * Math.Tan(QuarterPi - _halfChiOrigin);
                var h = (_rk4 * Math.Tan(Core.ChiOrigin)) + g;
                var i = Math.Atan(dx / (h + dy));
                var j = Math.Atan(dx / (g - dy)) - i;
                var chi = Core.ChiOrigin + 2.0 * Math.Atan(
                    (dy - (dx * Math.Tan(j / 2.0))) / Core.Rk2);
                var sinChi = Math.Sin(chi);
                var lambda = j + (2.0 * i) + Core.GeographicOrigin.Longitude;
                var lon = ((lambda - Core.GeographicOrigin.Longitude) / Core.N) + Core.GeographicOrigin.Longitude;
                var isometricLat = Math.Log((1 + sinChi) / (Core.C * (1 - sinChi))) / (2.0 * Core.N);
                var lat = (2.0 * Math.Atan(Math.Pow(Math.E, isometricLat))) - HalfPi;
                for (int convergeIndex = 0; convergeIndex < 8; convergeIndex++) {
                    var eSinLat = Core.E * Math.Sin(lat);
                    var isoLatI = Math.Log(
                        Math.Tan((lat / 2.0) + QuarterPi)
                        * Math.Pow((1 - eSinLat) / (1 + eSinLat), Core.EHalf)
                    );
                    var temp = lat - ((isoLatI - isometricLat) * Math.Cos(lat) * (1 - (eSinLat * eSinLat)) / (1.0 - Core.ESq));
                    if (temp == lat) break;
                    lat = temp;
                }
                return new GeographicCoordinate(lat, lon);
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double ScaleFactor;
        protected readonly double R;
        protected readonly double N;
        protected readonly double C;
        protected readonly double ChiOrigin;
        protected readonly double SinChiOrigin;
        protected readonly double CosChiOrigin;
        protected readonly double Rk2;

        public ObliqueStereographic(
            GeographicCoordinate geographicOrigin,
            double scaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geographicOrigin;
            ScaleFactor = scaleFactor;
            var sinLat = Math.Sin(GeographicOrigin.Latitude);
            var cosLat = Math.Cos(GeographicOrigin.Latitude);
            var oneMinusESquareSinLatSquare = 1 - (ESq * sinLat * sinLat);
            var po = (MajorAxis * (1 - ESq)) / Math.Pow(oneMinusESquareSinLatSquare, 1.5);
            var vo = MajorAxis / Math.Sqrt(oneMinusESquareSinLatSquare);
            R = Math.Sqrt(po * vo);
            N = Math.Sqrt(1 + ((ESq * cosLat * cosLat * cosLat * cosLat) / (1 - ESq)));
            var s1 = (1 + sinLat) / (1 - sinLat);
            var s2 = (1 - (E * sinLat)) / (1 + (E * sinLat));
            var w1 = Math.Pow(s1 * Math.Pow(s2, E), N);
            SinChiOrigin = (w1 - 1) / (w1 + 1);
            C = (N + sinLat) * (1 - SinChiOrigin) / ((N - sinLat) * (1 + SinChiOrigin));
            var w2 = C * w1;
            ChiOrigin = Math.Asin((w2 - 1) / (w2 + 1));
            SinChiOrigin = Math.Sin(ChiOrigin);
            CosChiOrigin = Math.Cos(ChiOrigin);
            Rk2 = R * scaleFactor * 2.0;
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var sinLat = Math.Sin(source.Latitude);
            var eSinLat = E * sinLat;
            var sa = (1 + sinLat) / (1 - sinLat);
            var sb = (1 - eSinLat) / (1 + eSinLat);
            var w = C * Math.Pow(sa * Math.Pow(sb, E), N);
            var chi = Math.Asin((w - 1) / (w + 1));
            var sinChi = Math.Sin(chi);
            //var sinChi = (w - 1) / (w + 1);
            var cosChi = Math.Cos(chi);

            var lambda = ((source.Longitude - GeographicOrigin.Longitude) * N) + GeographicOrigin.Longitude;
            var lambdaDelta = lambda - GeographicOrigin.Longitude;

            var beta = 1
                + (sinChi * SinChiOrigin)
                + (cosChi * CosChiOrigin * Math.Cos(lambdaDelta));

            var x = (Rk2 * cosChi * Math.Sin(lambdaDelta) / beta)
                + FalseProjectedOffset.X;
            var y = (Rk2 * (
                (sinChi * CosChiOrigin)
                - (cosChi * SinChiOrigin * Math.Cos(lambdaDelta))
            ) / beta) + FalseProjectedOffset.Y;
            return new Point2(x, y);
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new InvalidOperationException("No inverse.");
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get {
                return 0 != Rk2 && !Double.IsNaN(Rk2)
                    && 0 != N && !Double.IsNaN(N);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

    }
}
