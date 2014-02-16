using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class PseudoPlateCarree : ProjectionBase
    {

        private class Inverse : InvertedTransformationBase<PseudoPlateCarree, Point2, GeographicCoordinate>
        {

            public Inverse(PseudoPlateCarree core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                return new GeographicCoordinate(value.X, value.Y);
            }

        }

        public PseudoPlateCarree() { }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            return new Inverse(this);
        }

        public override Point2 TransformValue(GeographicCoordinate value) {
            return new Point2(value.Latitude, value.Longitude);
        }

        public override bool HasInverse {
            [Pure] get { return true; }
        }
    }
}
