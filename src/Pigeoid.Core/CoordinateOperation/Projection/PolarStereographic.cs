using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Periodic;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{

    public class PolarStereographic : SpheroidProjectionBase
    {

        private class Inverse : InvertedTransformationBase<PolarStereographic, Point2, GeographicCoordinate>
        {

            private readonly double _coefficient1;
            private readonly double _coefficient2;
            private readonly double _coefficient3;
            private readonly double _coefficient4;
            private readonly double _scaleValue;
            private readonly Func<Point2, GeographicCoordinate> _transform;

            public Inverse(PolarStereographic core)
                : base(core) {
                Contract.Requires(core != null);
                var e2 = Core.ESq;
                var e4 = e2 * e2;
                var e6 = e4 * e2;
                var e8 = e6 * e2;
                _coefficient1 = (e2 / 2) + (e4 * 5.0 / 24.0) + (e6 / 12) + (e8 * 12.0 / 360.0);
                _coefficient2 = (e4 * 7.0 / 48.0) + (e6 * 29.0 / 240.0) + (e8 * 811 / 11520);
                _coefficient3 = (e6 * 7.0 / 120.0) + (e8 * 81 / 1120);
                _coefficient4 = e8 * 4279.0 / 161280.0;
                _scaleValue = Core.CrazyEValue / (2.0 * Core.MajorAxis * Core.ScaleFactor);
                if (IsNorth(Core.GeographicOrigin.Latitude))
                    _transform = TransformValueNorth;
                else
                    _transform = TransformValueSouth;
            }

            private GeographicCoordinate TransformValueNorth(Point2 value) {
                var dx = value.X - Core.FalseProjectedOffset.X;
                var dy = value.Y - Core.FalseProjectedOffset.Y;
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (dx == 0 && dy == 0)
                    return Core.GeographicOrigin;
                // ReSharper restore CompareOfFloatsByEqualityOperator

                var chi = HalfPi - (
                    Math.Atan(
                        Math.Sqrt(
                            (dx * dx)
                            + (dy * dy)
                        ) * _scaleValue
                    ) * 2.0
                );

                return new GeographicCoordinate(
                    chi
                        + (_coefficient1 * Math.Sin(2 * chi))
                        + (_coefficient2 * Math.Sin(4 * chi))
                        + (_coefficient3 * Math.Sin(6 * chi))
                        + (_coefficient4 * Math.Sin(8 * chi)),
                    LongitudePeriod.Fix(
                        Math.Atan2(dx, -dy)
                        + Core.GeographicOrigin.Longitude)
                );
            }

            private GeographicCoordinate TransformValueSouth(Point2 value) {
                var dx = value.X - Core.FalseProjectedOffset.X;
                var dy = value.Y - Core.FalseProjectedOffset.Y;
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (dx == 0 && dy == 0)
                    return Core.GeographicOrigin;
                // ReSharper restore CompareOfFloatsByEqualityOperator

                var chi = (
                    Math.Atan(
                        Math.Sqrt(
                            (dx * dx)
                            + (dy * dy)
                        ) * _scaleValue
                    )
                    * 2.0
                ) - HalfPi;
                return new GeographicCoordinate(
                    chi
                        + (_coefficient1 * Math.Sin(2 * chi))
                        + (_coefficient2 * Math.Sin(4 * chi))
                        + (_coefficient3 * Math.Sin(6 * chi))
                        + (_coefficient4 * Math.Sin(8 * chi)),
                    LongitudePeriod.Fix(
                        Math.Atan2(dx, dy)
                        + Core.GeographicOrigin.Longitude)
                );
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                return _transform(value);
            }
        }

        private static readonly PeriodicOperations LongitudePeriod = new PeriodicOperations(-Math.PI, Math.PI * 2.0);

        public static PolarStereographic CreateFromStandardParallel(double standardParallel, double longitude, Vector2 falseProjectedOffset, ISpheroid<double> spheroid) {
            Contract.Requires(spheroid != null);
            Contract.Ensures(Contract.Result<PolarStereographic>() != null);
            return CreateFromStandardParallel(
                new GeographicCoordinate(standardParallel < 0 ? -Math.PI / 2.0 : Math.PI / 2.0, longitude),
                standardParallel,
                falseProjectedOffset,
                spheroid);
        }

        public static PolarStereographic CreateFromStandardParallel(GeographicCoordinate geographicOrigin, double standardParallel, Vector2 falseProjectedOffset, ISpheroid<double> spheroid) {
            Contract.Requires(spheroid != null);
            Contract.Ensures(Contract.Result<PolarStereographic>() != null);
            var sinLat = Math.Sin(standardParallel);
            var mf = Math.Cos(standardParallel) / Math.Sqrt(1 - (spheroid.ESquared * sinLat * sinLat));

            var scaleFactor = 1.0;
            if (Math.Abs(Math.Abs(standardParallel) - HalfPi) > Double.Epsilon) {
                var e = spheroid.E;
                var eSinLat = e * sinLat;
                var eHalf = e / 2.0;
                double tf;
                if (IsNorth(geographicOrigin.Latitude)) {
                    tf = Math.Tan(QuarterPi - (standardParallel / 2.0))
                        * Math.Pow((1 + eSinLat) / (1 - eSinLat), eHalf);
                }
                else {
                    tf = Math.Tan(QuarterPi + (standardParallel / 2.0))
                        / Math.Pow((1 + eSinLat) / (1 - eSinLat), eHalf);
                }
                scaleFactor = CalculateCrazyEValue(e) * mf / (2 * tf);
            }
            return new PolarStereographic(geographicOrigin, scaleFactor, falseProjectedOffset, spheroid);
        }

        public static PolarStereographic CreateFromStandardParallelAndFalseOffsetAtOrigin(double standardParallel, double longitude, Vector2 falseProjectedOffset, ISpheroid<double> spheroid) {
            Contract.Requires(spheroid != null);
            Contract.Ensures(Contract.Result<PolarStereographic>() != null);
            return CreateFromStandardParallelAndFalseOffsetAtOrigin(
                new GeographicCoordinate(standardParallel < 0 ? -Math.PI / 2.0 : Math.PI / 2.0, longitude),
                standardParallel,
                falseProjectedOffset,
                spheroid);
        }

        public static PolarStereographic CreateFromStandardParallelAndFalseOffsetAtOrigin(GeographicCoordinate geographicOrigin, double standardParallel, Vector2 falseProjectedOffsetAtOrigin, ISpheroid<double> spheroid) {
            Contract.Requires(spheroid != null);
            Contract.Ensures(Contract.Result<PolarStereographic>() != null);
            var sinLat = Math.Sin(standardParallel);
            var mf = Math.Cos(standardParallel) / Math.Sqrt(1 - (spheroid.ESquared * sinLat * sinLat));

            var pf = mf * spheroid.A;
            if (!IsNorth(geographicOrigin.Latitude))
                pf = -pf;

            return CreateFromStandardParallel(
                geographicOrigin,
                standardParallel,
                new Vector2(
                    falseProjectedOffsetAtOrigin.X,
                    pf + falseProjectedOffsetAtOrigin.Y
                ),
                spheroid
            );
        }

        private static double CalculateCrazyEValue(double e) {
            return Math.Sqrt(Math.Pow(1 + e, 1 + e) * Math.Pow(1 - e, 1 - e));
        }

        private static bool IsNorth(double latitude) {
            return latitude > 0;
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected double ScaleFactor;

        protected readonly double CrazyEValue;
        protected readonly double CoefficientT;
        private readonly Func<GeographicCoordinate, Point2> _transform;

        public PolarStereographic(GeographicCoordinate geographicOrigin, double scaleFactor, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
            : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            ScaleFactor = scaleFactor;
            GeographicOrigin = geographicOrigin;
            CrazyEValue = CalculateCrazyEValue(E);
            CoefficientT = MajorAxis * 2.0 * ScaleFactor / CrazyEValue;

            _transform = IsNorth(geographicOrigin.Latitude)
                ? (Func<GeographicCoordinate, Point2>)TransformValueNorth
                : TransformValueSouth;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_transform != null);
        }

        private Point2 TransformValueNorth(GeographicCoordinate source) {
            var theta = source.Longitude - GeographicOrigin.Longitude;
            var eSinLat = Math.Sin(source.Latitude) * E;
            var r = Math.Tan(QuarterPi - (source.Latitude / 2.0))
                    * Math.Pow((1 + eSinLat) / (1 - eSinLat), EHalf)
                    * CoefficientT;
            var de = r * Math.Sin(theta);
            var dn = r * Math.Cos(theta);
            return new Point2(de + FalseProjectedOffset.X, FalseProjectedOffset.Y - dn);
        }

        private Point2 TransformValueSouth(GeographicCoordinate source) {
            var theta = source.Longitude - GeographicOrigin.Longitude;
            var eSinLat = Math.Sin(source.Latitude) * E;
            var r = (
                    Math.Tan((source.Latitude / 2.0) + QuarterPi)
                    / Math.Pow((1 + eSinLat) / (1 - eSinLat), EHalf)
                ) * CoefficientT;
            var de = r * Math.Sin(theta);
            var dn = r * Math.Cos(theta);
            return new Point2(de + FalseProjectedOffset.X, dn + FalseProjectedOffset.Y);
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            return _transform(source);
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if(!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public override bool HasInverse {
            [Pure] get {
                return 0 != MajorAxis && !Double.IsNaN(MajorAxis)
                    && 0 != ScaleFactor && !Double.IsNaN(ScaleFactor);
            }
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

    }
}
