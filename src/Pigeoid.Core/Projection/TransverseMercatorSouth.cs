using System.Diagnostics;
using System.Diagnostics.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
    public class TransverseMercatorSouth :
        TransverseMercator
    {

        private class Inverted : InvertedTransformationBase<TransverseMercatorSouth, Point2, GeographicCoordinate>
        {

            private readonly InvertedTransformationBase<TransverseMercator, Point2, GeographicCoordinate> _baseInv;

            public Inverted(TransverseMercatorSouth core) : base(core) {
                Contract.Requires(core != null);
                Contract.Requires(core.HasInverse);
                _baseInv = core.BaseInverse;
            }

            public override GeographicCoordinate TransformValue(Point2 source) {
                return _baseInv.TransformValue(new Point2(-source.Y, -source.X));
            }
        }

        public TransverseMercatorSouth(
            GeographicCoordinate naturalOrigin,
            Vector2 falseProjectedOffset,
            double scaleFactor,
            ISpheroid<double> spheroid
        ) : base(
            naturalOrigin,
            new Vector2(-falseProjectedOffset.X, -falseProjectedOffset.Y),
            scaleFactor,
            spheroid
        ) {
            Contract.Requires(spheroid != null);
        }

        public override Point2 TransformValue(GeographicCoordinate coordinate) {
            var p = base.TransformValue(coordinate);
            return new Point2(-p.Y, -p.X);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private InvertedTransformationBase<TransverseMercator, Point2, GeographicCoordinate> BaseInverse {
            get {
                Contract.Requires(base.HasInverse);
                Contract.Ensures(Contract.Result<InvertedTransformationBase<TransverseMercator, Point2, GeographicCoordinate>>() != null);
                return (InvertedTransformationBase<TransverseMercator, Point2, GeographicCoordinate>)(base.GetInverse());
            }
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

    }
}
