using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class LambertAzimuthalEqualArea : SpheroidProjectionBase
    {

        private class Inverted : InvertedTransformationBase<LambertAzimuthalEqualArea, Point2, GeographicCoordinate>
        {

            public Inverted(LambertAzimuthalEqualArea core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 source) {
                var xDelta = source.X - Core.FalseProjectedOffset.X;
                var yDelta = source.Y - Core.FalseProjectedOffset.Y;
                var dYDelta = Core.D * yDelta;
                var radiusOfCurvature = new Vector2(xDelta / Core.D, Core.D * dYDelta).GetMagnitude();
                var c = 2.0 * Math.Asin(radiusOfCurvature / (2.0 * Core.RadiusQ));
                var cosC = Math.Cos(c);
                var sinC = Math.Sin(c);
                var beta = Math.Asin(
                    (cosC * Core.SinBetaOrigin)
                    + ((dYDelta * sinC * Core.CosBetaOrigin) / radiusOfCurvature)
                );

                var e4 = Core.ESq * Core.ESq;
                var e6 = e4 * Core.ESq;
                var lat = beta
                    + (((Core.ESq / 3.0) + (e4 * 31.0 / 180.0) + (e6 * 517.0 / 5040)) * Math.Sin(2 * beta))
                    + (((e4 / 23.0 / 360.0) + (e6 * 251.0 / 3780.0)) * Math.Sin(4 * beta))
                    + ((e6 * 761.0 / 45360.0) * Math.Sin(6 * beta));
                var lon = Core.GeographicOrigin.Longitude + Math.Atan(
                    xDelta * sinC / (
                        (Core.D * radiusOfCurvature * Core.CosBetaOrigin * cosC)
                        - (Core.D * Core.D * yDelta * Core.SinBetaOrigin * sinC)
                    )
                );
                return new GeographicCoordinate(lat, lon);
            }
        }

        public readonly GeographicCoordinate GeographicOrigin;

        protected readonly double QParallel;
        protected readonly double SinBetaOrigin;
        protected readonly double CosBetaOrigin;
        protected readonly double RadiusQ;
        protected readonly double D;
        protected readonly double OneMinusESq;
        protected readonly double OneMinusESqSinLatOriginSq;
        protected readonly double OneOverTwoE;

        public LambertAzimuthalEqualArea(
            GeographicCoordinate geogOrigin,
            Vector2 falseOffset,
            ISpheroid<double> spheroid
        ) : base(falseOffset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geogOrigin;
            OneMinusESq = 1 - ESq;
            var sinLatOrigin = Math.Sin(geogOrigin.Latitude);
            var cosLatOrigin = Math.Cos(geogOrigin.Latitude);
            var eSqSinLatOriginSq = ESq * sinLatOrigin * sinLatOrigin;
            OneMinusESqSinLatOriginSq = 1.0 - eSqSinLatOriginSq;
            OneOverTwoE = 1.0 / (2.0 * E);
            var sinLatOriginE = sinLatOrigin * E;
            var qOrigin = OneMinusESq * (
                (sinLatOrigin / OneMinusESqSinLatOriginSq)
                - (OneOverTwoE * Math.Log((1 - sinLatOriginE) / (1 + sinLatOriginE)))
            );
            QParallel = OneMinusESq * (
                (1 / OneMinusESq)
                - (OneOverTwoE * Math.Log((1 - E) / (1 + E)))
            );
            RadiusQ = MajorAxis * Math.Sqrt(QParallel / 2.0);

            var betaOrigin = Math.Asin(qOrigin / QParallel);
            SinBetaOrigin = Math.Sin(betaOrigin);
            CosBetaOrigin = Math.Cos(betaOrigin);
            D = MajorAxis
                * (cosLatOrigin / Math.Sqrt(1 - eSqSinLatOriginSq))
                / (RadiusQ * CosBetaOrigin);
            ;
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var sinLat = Math.Sin(source.Latitude);
            var eSinLat = sinLat * E;
            var q = OneMinusESq * (
                (sinLat / (1 - (ESq * sinLat * sinLat)))
                - (OneOverTwoE * Math.Log((1 - eSinLat) / (1 + eSinLat)))
            );
            var beta = Math.Asin(q / QParallel);
            var sinBeta = Math.Sin(beta);
            var cosBeta = Math.Cos(beta);
            var deltaLon = source.Longitude - GeographicOrigin.Longitude;
            var cosDeltaLon = Math.Cos(deltaLon);
            var b = RadiusQ * Math.Sqrt(
                2.0 / (
                    1
                    + (SinBetaOrigin * sinBeta)
                    + (CosBetaOrigin * cosBeta * cosDeltaLon)
                )
            );
            return new Point2(
                FalseProjectedOffset.X + (b * D * cosBeta * Math.Sin(deltaLon)),
                FalseProjectedOffset.Y + (
                    (b / D)
                    * (
                        (CosBetaOrigin * sinBeta)
                        - (SinBetaOrigin * cosBeta * cosDeltaLon)
                    )
                )
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            [Pure] get {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                return 0 != D && !Double.IsNaN(D);
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }

    }
}
