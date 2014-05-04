using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{

    public class LambertConicNearConformal : LambertConicBase
    {

        private class Inverted : InvertedTransformationBase<LambertConicNearConformal, Point2, GeographicCoordinate>
        {

            private readonly bool _negateR;
            private readonly double _northOffsetBase;
            private readonly double _scaledA;

            public Inverted(LambertConicNearConformal core) : base(core) {
                Contract.Requires(core != null);
                _negateR = core.GeographicOrigin.Latitude < 0;
                _northOffsetBase = core.FalseProjectedOffset.Y + core.ROrigin;
                _scaledA = core.OriginScaleFactor * core.A;
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                var eastOffset = value.X - Core.FalseProjectedOffset.X;
                var northOffset = _northOffsetBase - value.Y;
                var mBase = Math.Sqrt(
                    (eastOffset * eastOffset)
                    + (northOffset * northOffset));
                var m = mBase = _negateR
                    ? Core.ROrigin + mBase
                    : Core.ROrigin - mBase;
                int i;
                double old;
                // converge m
                for (i = 0; i < 16; i++) {
                    old = m;
                    var m2CoreAScaled = _scaledA * m * m;
                    m = m - (
                        (
                            mBase
                            - (Core.OriginScaleFactor * m)
                            - (m2CoreAScaled * m)
                        )
                        /
                        (
                            -Core.OriginScaleFactor
                            - (3.0 * m2CoreAScaled)
                        )
                    );
                    if (old == m)
                        break;
                }
                // converge latitude
                var lat = ((m / Core.ConstantA) * (Math.PI / 180))
                    + Core.GeographicOrigin.Latitude;
                for (i = 0; i < 16; i++) {
                    old = lat;
                    lat = (
                        (
                            (
                                (
                                    (Math.Sin(2.0 * lat) * Core.ConstantB)
                                    + (Math.Sin(6.0 * lat) * Core.ConstantD)
                                    - ((180.0 / Math.PI) * lat * Core.ConstantA)
                                    - (Math.Sin(4.0 * lat) * Core.ConstantC)
                                    - (Math.Sin(8.0 * lat) * Core.ConstantE)
                                )
                                + m
                                + Core.SOrigin
                            ) / Core.ConstantA
                        )
                        * (Math.PI / 180.0)
                    ) + lat;
                    if (old == lat)
                        break;
                }

                return new GeographicCoordinate(
                    lat,
                    (Math.Atan(eastOffset / northOffset) / Core.SinOriginLatitude)
                    + Core.GeographicOrigin.Longitude
                );
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Origin scale factor.
        /// </summary>
        public readonly double OriginScaleFactor;

        protected double A;
        protected double N;
        protected double ROrigin;
        protected double SOrigin;
        protected double SinOriginLatitude;
        protected double ConstantA;
        protected double ConstantB;
        protected double ConstantC;
        protected double ConstantD;
        protected double ConstantE;

        public LambertConicNearConformal(
            GeographicCoordinate geographicOrigin,
            double originScaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(geographicOrigin, falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            OriginScaleFactor = originScaleFactor;

            N = spheroid.F / (2 - spheroid.F); // TODO: better way to calculate this?
            var n2 = N * N;
            var n3 = n2 * N;
            var n4 = n3 * N;
            var n5 = n4 * N;

            SinOriginLatitude = Math.Sin(geographicOrigin.Latitude);
            var p0 = (spheroid.A * (1.0 - spheroid.ESquared))
                / Math.Pow(1.0 - (spheroid.ESquared * SinOriginLatitude * SinOriginLatitude), 1.5);
            var v0 = spheroid.A / Math.Sqrt(1.0 - (spheroid.ESquared * SinOriginLatitude * SinOriginLatitude));

            A = 1.0 / (6.0 * p0 * v0);

            ConstantA = spheroid.A * (
                1.0 - N + ((5.0 * (n2 - n3)) / 4.0) + ((81.0 * (n4 - n5)) / 64.0)
                ) * (Math.PI / 180.0);
            ConstantB = 3.0 * spheroid.A * (
                N - n2 + ((7.0 * (n3 - n4)) / 8.0) + ((55.0 * n5) / 64.0)
                ) / 2.0;
            ConstantC = 15.0 * spheroid.A * (
                n2 - n3 + ((3.0 * (n4 - n5)) / 4.0)
                ) / 16.0;
            ConstantD = 35.0 * spheroid.A * (
                n3 - n4 + ((11 * n5) / 16.0)
                ) / 48.0;
            ConstantE = 315.0 * spheroid.A * (n4 - n5) / 512.0;

            ROrigin = (OriginScaleFactor * v0) / Math.Tan(geographicOrigin.Latitude);

            SOrigin = (ConstantA * (geographicOrigin.Latitude * 180.0 / Math.PI)) // NOTE: first term is multiplied vs degrees, may be a way to optimize this!
                - (ConstantB * Math.Sin(2.0 * geographicOrigin.Latitude))
                + (ConstantC * Math.Sin(4.0 * geographicOrigin.Latitude))
                - (ConstantD * Math.Sin(6.0 * geographicOrigin.Latitude))
                + (ConstantE * Math.Sin(8.0 * geographicOrigin.Latitude));

        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var s = (source.Latitude * (180.0 / Math.PI) * ConstantA) // NOTE: first term is multiplied vs degrees, may be a way to optimize this!
                - (Math.Sin(2.0 * source.Latitude) * ConstantB)
                + (Math.Sin(4.0 * source.Latitude) * ConstantC)
                - (Math.Sin(6.0 * source.Latitude) * ConstantD)
                + (Math.Sin(8.0 * source.Latitude) * ConstantE);

            var m = s - SOrigin;
            var mResult = ((m * m * m * A) + m) * OriginScaleFactor;
            var r = ROrigin - mResult;

            var theta = (source.Longitude - GeographicOrigin.Longitude) * SinOriginLatitude;
            var sinTheta = Math.Sin(theta);
            var x = (r * sinTheta) + FalseProjectedOffset.X;
            var y = (Math.Tan(theta / 2.0) * r * sinTheta) + FalseProjectedOffset.Y + mResult;
            return new Point2(x, y);
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public override bool HasInverse {
            [Pure] get {
                return 0 != SinOriginLatitude && !Double.IsNaN(SinOriginLatitude)
                    && 0 != ConstantA && !Double.IsNaN(ConstantA);
            }
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

    }
}
