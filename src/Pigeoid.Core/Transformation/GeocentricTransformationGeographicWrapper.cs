using System;
using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class GeocentricTransformationGeographicWrapper<T> :
		GeographicGeocentricTransformationBase
		where T:ITransformation<Point3>
	{

		private class Inverse : GeographicGeocentricTransformationBase
		{

			private readonly GeocentricTransformationGeographicWrapper<T> _coreWrapper;
			private readonly ITransformation<Point3> _inverseOperation;

			public Inverse([NotNull] GeocentricTransformationGeographicWrapper<T> coreWrapper)
				: base(coreWrapper.GeocentricToGeographic.GetInverse(), coreWrapper.GeographicToGeocentric.GetInverse())
			{
				_coreWrapper = coreWrapper;
				_inverseOperation = coreWrapper.GeocentricCore.GetInverse();
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

		public GeocentricTransformationGeographicWrapper([NotNull] GeographicGeocentricTransformation geographicGeocentric, [NotNull] GeocentricGeographicTransformation geocentricGeographic, [NotNull] T core) : base(geographicGeocentric,geocentricGeographic) {
			if (null == core)
				throw new ArgumentNullException("core");
			
			_core = core;
		}

		public GeocentricTransformationGeographicWrapper([NotNull] ISpheroid<double> fromSpheroid, [NotNull] ISpheroid<double> toSpheroid, [NotNull] T core)
			: base(fromSpheroid, toSpheroid) {
			if (null == core)
				throw new ArgumentNullException("core");
			
			_core = core;
		}

		public T GeocentricCore { get { return _core; } }

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
			if(!HasInverse)
				throw new InvalidOperationException("No inverse.");
			return new Inverse(this);
		}
	}
}
