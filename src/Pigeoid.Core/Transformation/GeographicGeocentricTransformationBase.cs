using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public abstract class GeographicGeocentricTransformationBase :
		ITransformation<GeographicCoordinate>,
		ITransformation<GeographicHeightCoordinate>,
		ITransformation<GeographicCoordinate, GeographicHeightCoordinate>,
		ITransformation<GeographicHeightCoordinate, GeographicCoordinate>
	{

		protected GeographicGeocentricTransformationBase([NotNull] ISpheroid<double> spheroidFrom, [NotNull] ISpheroid<double> spheroidTo)
			:this(new GeographicGeocentricTransformation(spheroidFrom),new GeocentricGeographicTransformation(spheroidTo)) { }

		protected GeographicGeocentricTransformationBase([NotNull] GeographicGeocentricTransformation geographicGeocentric, [NotNull] GeocentricGeographicTransformation geocentricGeographic) {
			if(null == geographicGeocentric)
				throw new ArgumentNullException("geographicGeocentric");
			if(null == geocentricGeographic)
				throw new ArgumentNullException("geocentricGeographic");
			GeographicToGeocentric = geographicGeocentric;
			GeocentricToGeographic = geocentricGeographic;
		}

		protected GeographicGeocentricTransformation GeographicToGeocentric { get; private set; }
		protected GeocentricGeographicTransformation GeocentricToGeographic { get; private set; }

		public ISpheroid<double> SpheroidFrom { get { return GeographicToGeocentric.Spheroid; } }
		public ISpheroid<double> SpheroidTo { get { return GeocentricToGeographic.Spheroid; } }

		public abstract GeographicCoordinate TransformValue(GeographicCoordinate value);
		public abstract GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value);
		public abstract GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value);
		public abstract GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value);

		public IEnumerable<GeographicCoordinate> TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public void TransformValues([NotNull] GeographicCoordinate[] values) {
			for (var i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}

		public IEnumerable<GeographicHeightCoordinate> TransformValues([NotNull] IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}

		public void TransformValues([NotNull] GeographicHeightCoordinate[] values) {
			for (var i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}

		IEnumerable<GeographicHeightCoordinate> ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue3D);
		}

		GeographicHeightCoordinate ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.TransformValue(GeographicCoordinate value) {
			return TransformValue3D(value);
		}

		IEnumerable<GeographicCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.TransformValues([NotNull] IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue2D);
		}

		GeographicCoordinate ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.TransformValue(GeographicHeightCoordinate value) {
			return TransformValue2D(value);
		}

		public abstract ITransformation GetInverse();

		ITransformation<GeographicCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.GetInverse() {
			return (ITransformation<GeographicCoordinate, GeographicHeightCoordinate>)GetInverse();
		}

		ITransformation<GeographicHeightCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.GetInverse() {
			return (ITransformation<GeographicHeightCoordinate, GeographicCoordinate>)GetInverse();
		}

		ITransformation<GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate>.GetInverse() {
			return (ITransformation<GeographicHeightCoordinate>)GetInverse();
		}

		ITransformation<GeographicCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicCoordinate>.GetInverse() {
			return (ITransformation<GeographicCoordinate, GeographicCoordinate>)GetInverse();
		}

		ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>.GetInverse() {
			return (ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>)GetInverse();
		}

		ITransformation<GeographicCoordinate> ITransformation<GeographicCoordinate>.GetInverse() {
			return (ITransformation<GeographicCoordinate>)GetInverse();
		}

		public virtual bool HasInverse {
			get {
				return GeographicToGeocentric.HasInverse
					&& GeocentricToGeographic.HasInverse;
			}
		}

	}
}
