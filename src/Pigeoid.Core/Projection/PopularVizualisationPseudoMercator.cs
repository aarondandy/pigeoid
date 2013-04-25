using System;
using System.Diagnostics.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
    public class PopularVisualizationPseudoMercator : ProjectionBase
    {

        private class Inverse : InvertedTransformationBase<PopularVisualizationPseudoMercator, Point2, GeographicCoordinate>
        {

            public Inverse(PopularVisualizationPseudoMercator core) : base(core) {Contract.Requires(core != null);}

            public override GeographicCoordinate TransformValue(Point2 value) {
                return new GeographicCoordinate(
                    (Math.Atan(Math.Pow(Math.E, (Core.FalseProjectedOffset.Y - value.Y) / Core.R)) * -2.0) + HalfPi,
                    ((value.X - Core.FalseProjectedOffset.X) / Core.R) + Core.GeographicOrigin.Longitude
                );
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        private readonly double R;

        public PopularVisualizationPseudoMercator(
            GeographicCoordinate geographicOrigin,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geographicOrigin;
            R = spheroid.A;
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            return new Point2(
                ((source.Longitude - GeographicOrigin.Longitude) * R) + FalseProjectedOffset.X,
                (Math.Log(Math.Tan((source.Latitude / 2.0) + QuarterPi)) * R) + FalseProjectedOffset.Y
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new InvalidOperationException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public override bool HasInverse { get { return 0 != R && !Double.IsNaN(R); } }
        // ReSharper restore CompareOfFloatsByEqualityOperator

    }
}
