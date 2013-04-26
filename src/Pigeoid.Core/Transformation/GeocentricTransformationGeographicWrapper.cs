using System;
using System.Diagnostics.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
    public class GeocentricTransformationGeographicWrapper<T> :
        GeographicGeocentricTransformationBase
        where T : ITransformation<Point3>
    {

        private class Inverse : GeographicGeocentricTransformationBase
        {

            private readonly GeocentricTransformationGeographicWrapper<T> _coreWrapper;
            private readonly ITransformation<Point3> _inverseOperation;

            public Inverse(GeocentricTransformationGeographicWrapper<T> coreWrapper)
                : base(coreWrapper.GeocentricToGeographic.GetInverse(), coreWrapper.GeographicToGeocentric.GetInverse()) {
                Contract.Requires(coreWrapper != null);
                _coreWrapper = coreWrapper;
                _inverseOperation = coreWrapper.GeocentricCore.GetInverse();
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_coreWrapper != null);
                Contract.Invariant(_inverseOperation != null);
            }

            public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
                return GeocentricToGeographic.TransformValue2D(_inverseOperation.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
                return GeocentricToGeographic.TransformValue(_inverseOperation.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
                return GeocentricToGeographic.TransformValue(_inverseOperation.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
                return GeocentricToGeographic.TransformValue2D(_inverseOperation.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override ITransformation GetInverse() {
                return _coreWrapper;
            }

            public override bool HasInverse {
                get { return true; }
            }

        }

        private readonly T _core;

        public GeocentricTransformationGeographicWrapper(GeographicGeocentricTransformation geographicGeocentric, GeocentricGeographicTransformation geocentricGeographic, T core)
            : base(geographicGeocentric, geocentricGeographic) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.Requires(geographicGeocentric != null);
            Contract.Requires(geocentricGeographic != null);
            _core = core;
        }

        public GeocentricTransformationGeographicWrapper(ISpheroid<double> fromSpheroid, ISpheroid<double> toSpheroid, T core)
            : base(fromSpheroid, toSpheroid) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.Requires(fromSpheroid != null);
            Contract.Requires(toSpheroid != null);
            _core = core;
        }

        public T GeocentricCore {
            get {
                Contract.Ensures(Contract.Result<T>() != null);
                return _core;
            }
        }

        public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(_core.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue(_core.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue(_core.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(_core.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override bool HasInverse {
            get {
                return base.HasInverse && _core.HasInverse;
            }
        }

        public override ITransformation GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation>() != null);
            return new Inverse(this);
        }
    }
}
