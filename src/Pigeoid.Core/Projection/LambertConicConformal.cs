using System;
using System.Diagnostics.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
    /// <summary>
    /// A base class for Lambert Conic Conformal projections.
    /// </summary>
    public abstract class LambertConicConformal : LambertConicBase
    {

        internal const string DefaultName = "Lambert Conformal Conic";

        protected double Af;
        protected double N;
        protected double F;
        protected double ROrigin;
        protected double InvN;
        protected double NorthingOffset;

        private class Inverted : InvertedTransformationBase<LambertConicConformal, Point2, GeographicCoordinate>
        {

            public Inverted(LambertConicConformal core) : base(core) {
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
                    temp = HalfPi - (
                        2.0 * Math.Atan(
                            t
                            * Math.Pow(
                                (1 - temp) / (1 + temp),
                                Core.EHalf
                            )
                        )
                    );

                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    if (temp == lat) break;
                    // ReSharper restore CompareOfFloatsByEqualityOperator

                    lat = temp;
                }
                return new GeographicCoordinate(
                    lat,
                    (Math.Atan(eastingComponent / northingComponent) / Core.N)
                        + Core.GeographicOrigin.Longitude
                );
            }

        }

        protected LambertConicConformal(
            GeographicCoordinate geographicOrigin,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(geographicOrigin, falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
        }

        public override Point2 TransformValue(GeographicCoordinate coordinate) {
            var r = E * Math.Sin(coordinate.Latitude);
            r = Af * Math.Pow(
                (
                    Math.Tan(
                        QuarterPi
                        -
                        (coordinate.Latitude / 2.0)
                    )
                    /
                    Math.Pow(
                        (1.0 - r) / (1.0 + r),
                        EHalf
                    )
                ),
                N
            );
            var theta = N * (coordinate.Longitude - GeographicOrigin.Longitude);
            return new Point2(
                (Math.Sin(theta) * r) + FalseProjectedOffset.X,
                NorthingOffset - (Math.Cos(theta) * r)
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
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


    }
}
