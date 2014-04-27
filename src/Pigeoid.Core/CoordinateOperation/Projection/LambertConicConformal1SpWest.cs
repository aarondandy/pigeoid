using System.Diagnostics;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class LambertConicConformal1SpWest :
        LambertConicConformal1Sp
    {

        private class Inverted : InvertedTransformationBase<LambertConicConformal1SpWest, Point2, GeographicCoordinate>
        {

            private readonly InvertedTransformationBase<LambertConicConformal, Point2, GeographicCoordinate> _baseInv;

            public Inverted(LambertConicConformal1SpWest core) : base(core) {
                Contract.Requires(core != null);
                Contract.Requires(core.HasInverse);
                _baseInv = core.BaseInverse;
            }

            [ContractInvariantMethod]
            private void ObjectInvariants() {
                Contract.Invariant(_baseInv != null);
            }

            public override GeographicCoordinate TransformValue(Point2 source) {
                return _baseInv.TransformValue(new Point2(-source.X, source.Y));
            }
        }

        public LambertConicConformal1SpWest(
            GeographicCoordinate geographicOrigin,
            double originScaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(
            geographicOrigin,
            originScaleFactor,
            new Vector2(-falseProjectedOffset.X, falseProjectedOffset.Y),
            spheroid
        ) {
            Contract.Requires(spheroid != null);
        }

        public override Point2 TransformValue(GeographicCoordinate coordinate) {
            var p = base.TransformValue(coordinate);
            return new Point2(-p.X, p.Y);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private InvertedTransformationBase<LambertConicConformal, Point2, GeographicCoordinate> BaseInverse {
            get {
                Contract.Requires(HasInverse);
                Contract.Ensures(Contract.Result<InvertedTransformationBase<LambertConicConformal, Point2, GeographicCoordinate>>() != null);
                return (InvertedTransformationBase<LambertConicConformal, Point2, GeographicCoordinate>)(base.GetInverse());
            }
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

    }
}
