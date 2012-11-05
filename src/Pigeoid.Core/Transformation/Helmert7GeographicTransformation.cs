using System;
using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class Helmert7GeographicTransformation : GeographicGeocentricTransformationBase
	{

		private class Inverse : GeographicGeocentricTransformationBase
		{

			private readonly Helmert7GeographicTransformation _core;
			private readonly ITransformation<Point3> _helmertInverse;

			public Inverse([NotNull] Helmert7GeographicTransformation core)
				: base(core.GeocentricToGeographic.GetInverse(), core.GeographicToGeocentric.GetInverse())
			{
				_core = core;
				_helmertInverse = _core.Helmert.GetInverse();
			}

			public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
				return GeocentricToGeographic.TransformValue2D(_helmertInverse.TransformValue(GeographicToGeocentric.TransformValue(value)));
			}

			public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
				return GeocentricToGeographic.TransformValue(_helmertInverse.TransformValue(GeographicToGeocentric.TransformValue(value)));
			}

			public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
				return GeocentricToGeographic.TransformValue(_helmertInverse.TransformValue(GeographicToGeocentric.TransformValue(value)));
			}

			public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
				return GeocentricToGeographic.TransformValue2D(_helmertInverse.TransformValue(GeographicToGeocentric.TransformValue(value)));
			}

			public override bool HasInverse { get { return true; } }

			public override ITransformation GetInverse(){
				return _core;
			}
		}

		public Helmert7GeographicTransformation([NotNull] ISpheroid<double> spheroidFrom, [NotNull] Helmert7Transformation helmert, [NotNull] ISpheroid<double> spheroidTo)
			: base(spheroidFrom, spheroidTo)
		{
			if(null == helmert)
				throw new ArgumentNullException("helmert");
			Helmert = helmert;
		}

		public Helmert7Transformation Helmert { get; private set; }

		public override GeographicCoordinate TransformValue(GeographicCoordinate value){
			return GeocentricToGeographic.TransformValue2D(Helmert.TransformValue(GeographicToGeocentric.TransformValue(value)));
		}

		public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
			return GeocentricToGeographic.TransformValue(Helmert.TransformValue(GeographicToGeocentric.TransformValue(value)));
		}

		public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
			return GeocentricToGeographic.TransformValue(Helmert.TransformValue(GeographicToGeocentric.TransformValue(value)));
		}

		public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
			return GeocentricToGeographic.TransformValue2D(Helmert.TransformValue(GeographicToGeocentric.TransformValue(value)));
		}

		public override ITransformation GetInverse() {
			if(!HasInverse)
				throw new InvalidOperationException("No inverse.");
			return new Inverse(this);
		}

		public override bool HasInverse {
			get { return base.HasInverse && Helmert.HasInverse; }
		}

	}
}
