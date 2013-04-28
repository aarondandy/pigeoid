using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.CoordinateOperation.Projection
{
    /// <summary>
    /// A Lambert Conic Conformal Belgium projection.
    /// </summary>
    public class LambertConicConformalBelgium : LambertConicConformal2Sp
    {

        private const double ThetaOffset = 0.00014204313635987739;

        private class Inverted : InvertedTransformationBase<LambertConicConformalBelgium, Point2, GeographicCoordinate>
        {

            public Inverted(LambertConicConformalBelgium core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 coordinate) {
                var eastingComponent = coordinate.X - Core.FalseProjectedOffset.X;
                var northingComponent = Core.NorthingOffset - coordinate.Y;
                var t = Math.Sqrt((eastingComponent * eastingComponent) + (northingComponent * northingComponent));
                t = Math.Pow(((Core.N < 0) ? -t : t) / Core.Af, Core.InvN);
                var lat = HalfPi;
                for (int i = 0; i < 8; i++) {
                    var temp = Core.E * Math.Sin(lat);
                    temp = HalfPi - (2.0 * Math.Atan(t * Math.Pow((1 - temp) / (1 + temp), Core.EHalf)));

                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    if (temp == lat) break;
                    // ReSharper restore CompareOfFloatsByEqualityOperator

                    lat = temp;
                }
                return new GeographicCoordinate(
                    lat,
                    ((Math.Atan(eastingComponent / northingComponent) + ThetaOffset) / Core.N) + Core.GeographicOrigin.Longitude
                );
            }

        }

        /// <summary>
        /// Constructs a new Lambert Conic Conformal Belgium projection.
        /// </summary>
        /// <param name="geographicOrigin">The geographic origin.</param>
        /// <param name="firstParallel">The first parallel.</param>
        /// <param name="secondParallel">The second parallel.</param>
        /// <param name="falseProjectedOffset">The false projected offset.</param>
        /// <param name="spheroid">The spheroid.</param>
        public LambertConicConformalBelgium(
            GeographicCoordinate geographicOrigin,
            double firstParallel,
            double secondParallel,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(geographicOrigin, firstParallel, secondParallel, falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if(!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get {
                return 0 != Af && !Double.IsNaN(Af)
                    && 0 != N && !Double.IsNaN(N);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override Point2 TransformValue(GeographicCoordinate coordinate) {
            var r = E * Math.Sin(coordinate.Latitude);
            r = Af * Math.Pow(
                Math.Tan(QuarterPi - (coordinate.Latitude / 2.0))
                / Math.Pow((1.0 - r) / (1.0 + r), EHalf),
                N
            );
            var theta = (N * (coordinate.Longitude - GeographicOrigin.Longitude)) - ThetaOffset;
            return new Point2(
                FalseProjectedOffset.X + (r * Math.Sin(theta)),
                NorthingOffset - (r * Math.Cos(theta))
            );
        }

    }
}
