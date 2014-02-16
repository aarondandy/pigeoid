using System;
using System.Diagnostics.Contracts;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class KrovakModifiedNorth : ProjectionBase
    {

        protected readonly KrovakModified Core;

        public KrovakModifiedNorth(
            GeographicCoordinate geographicOrigin,
            double latitudeOfPseudoStandardParallel,
            double azimuthOfInitialLine,
            double scaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid,
            Point2 evaluationPoint,
            double[] constants
        ) : this(new KrovakModified(
            geographicOrigin,
            latitudeOfPseudoStandardParallel,
            azimuthOfInitialLine,
            scaleFactor,
            falseProjectedOffset,
            spheroid,
            evaluationPoint,
            constants
        ))
        {
            Contract.Requires(spheroid != null);
            Contract.Requires(constants != null);
            Contract.Requires(constants.Length == 10);
        }

        public KrovakModifiedNorth(KrovakModified core) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            Core = core;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Core != null);
        }

        internal class Inverse : KrovakModified.Inverse
        {
            public Inverse(KrovakModified core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                return base.TransformValue(new Point2(-value.Y, -value.X));
            }
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(Core);
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var p = Core.TransformValue(source);
            return new Point2(-p.Y, -p.X);
        }

        public string Name {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return "Krovak Modified North";
            }
        }

        public override bool HasInverse {
            [Pure] get { return Core.HasInverse; }
        }

    }
}
