using System;
using System.Diagnostics.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
    public class MolodenskyBadekasGeographicTransformation : GeographicGeocentricTransformationBase
    {

        private class Inverse : GeographicGeocentricTransformationBase
        {
            private readonly MolodenskyBadekasGeographicTransformation _core;
            private readonly ITransformation<Point3> _mbInverse;

            public Inverse(MolodenskyBadekasGeographicTransformation core)
                : base(core.GeocentricToGeographic.GetInverse(), core.GeographicToGeocentric.GetInverse()) {
                _core = core;
                Contract.Requires(core != null);
                _mbInverse = _core.MolodenskyBadekas.GetInverse();
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_core != null);
                Contract.Invariant(_mbInverse != null);
            }

            public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
                return GeocentricToGeographic.TransformValue2D(_mbInverse.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
                return GeocentricToGeographic.TransformValue(_mbInverse.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
                return GeocentricToGeographic.TransformValue(_mbInverse.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
                return GeocentricToGeographic.TransformValue2D(_mbInverse.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override bool HasInverse { get { return true; } }

            public override ITransformation GetInverse() {
                return _core;
            }
        }

        public MolodenskyBadekasGeographicTransformation(ISpheroid<double> spheroidFrom, MolodenskyBadekasTransformation molodenskyBadekas, ISpheroid<double> spheroidTo)
            : base(spheroidFrom, spheroidTo) {
            if (null == molodenskyBadekas) throw new ArgumentNullException("molodenskyBadekas");
            Contract.Requires(spheroidFrom != null);
            Contract.Requires(spheroidTo != null);
            MolodenskyBadekas = molodenskyBadekas;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(MolodenskyBadekas != null);
        }

        public MolodenskyBadekasTransformation MolodenskyBadekas { get; private set; }

        public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(MolodenskyBadekas.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue(MolodenskyBadekas.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue(MolodenskyBadekas.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(MolodenskyBadekas.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override ITransformation GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            [Pure] get { return base.HasInverse && MolodenskyBadekas.HasInverse; }
        }

    }
}
