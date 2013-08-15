using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class KrovakNorth : ITransformation<GeographicCoordinate, Point2>
    {

        protected readonly Krovak Core;

        public KrovakNorth(
            GeographicCoordinate geographicOrigin,
            double latitudeOfPseudoStandardParallel,
            double azimuthOfInitialLine,
            double scaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : this(new Krovak(
            geographicOrigin,
            latitudeOfPseudoStandardParallel,
            azimuthOfInitialLine,
            scaleFactor,
            falseProjectedOffset,
            spheroid
        ))
        {
            Contract.Requires(spheroid != null);
        }

        public KrovakNorth(Krovak core) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            Core = core;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Core != null);
        }

        internal class Inverse : Krovak.Inverse
        {
            public Inverse(Krovak core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                return base.TransformValue(new Point2(-value.Y, -value.X));
            }
        }

        public ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(Core);
        }

        public Point2 TransformValue(GeographicCoordinate source) {
            var p = Core.TransformValue(source);
            return new Point2(-p.Y, -p.X);
        }

        public string Name {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return "Krovak North";
            }
        }

        public IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values) {
            if(values == null) throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        public bool HasInverse {
            [Pure] get { return Core.HasInverse; }
        }
    }
}
