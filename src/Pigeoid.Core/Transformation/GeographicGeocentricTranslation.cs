using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class GeographicGeocentricTranslation : GeographicGeocentricTransformationBase
	{

		private readonly Vector3 _delta;

		public GeographicGeocentricTranslation([NotNull] ISpheroid<double> spheroidFrom, Vector3 delta, [NotNull] ISpheroid<double> spheroidTo)
			: base(new GeographicGeocentricTransformation(spheroidFrom), new GeocentricGeographicTransformation(spheroidTo))
		{
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
			return new GeographicGeocentricTranslation(SpheroidTo, _delta.GetNegative(), SpheroidFrom);
		}

	}
}
