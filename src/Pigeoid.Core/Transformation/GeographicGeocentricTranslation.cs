using System.Diagnostics.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
    public class GeographicGeocentricTranslation : GeographicGeocentricTransformationBase
    {

        private readonly Vector3 _delta;

        public GeographicGeocentricTranslation(ISpheroid<double> spheroidFrom, Vector3 delta, ISpheroid<double> spheroidTo)
            : base(new GeographicGeocentricTransformation(spheroidFrom), new GeocentricGeographicTransformation(spheroidTo)) {
            Contract.Requires(spheroidFrom != null);
            Contract.Requires(spheroidTo != null);
            _delta = delta;
        }

        public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(GeographicToGeocentric.TransformValue(value).Add(_delta));
        }

        public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue(GeographicToGeocentric.TransformValue(value).Add(_delta));
        }

        public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue(GeographicToGeocentric.TransformValue(value).Add(_delta));
        }

        public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(GeographicToGeocentric.TransformValue(value).Add(_delta));
        }

        public override ITransformation GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation>() != null);
            return new GeographicGeocentricTranslation(SpheroidTo, _delta.GetNegative(), SpheroidFrom);
        }

    }
}
