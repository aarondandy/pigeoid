using System.Diagnostics.Contracts;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class GeographicGeocentricTranslation : GeographicGeocentricTransformationBase
    {

        public GeographicGeocentricTranslation(ISpheroid<double> spheroidFrom, Vector3 delta, ISpheroid<double> spheroidTo)
            : base(new GeographicGeocentricTransformation(spheroidFrom), new GeocentricGeographicTransformation(spheroidTo)) {
            Contract.Requires(spheroidFrom != null);
            Contract.Requires(spheroidTo != null);
            Delta = delta;
        }

        public Vector3 Delta { get; private set; }

        public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(GeographicToGeocentric.TransformValue(value).Add(Delta));
        }

        public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue(GeographicToGeocentric.TransformValue(value).Add(Delta));
        }

        public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue(GeographicToGeocentric.TransformValue(value).Add(Delta));
        }

        public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(GeographicToGeocentric.TransformValue(value).Add(Delta));
        }

        public override ITransformation GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation>() != null);
            return new GeographicGeocentricTranslation(SpheroidTo, Delta.GetNegative(), SpheroidFrom);
        }

    }
}
