using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class GeographicGeocentricTranslation :
		ITransformation<GeographicCoordinate>,
		ITransformation<GeographicHeightCoordinate>,
		ITransformation<GeographicCoordinate,GeographicHeightCoordinate>,
		ITransformation<GeographicHeightCoordinate, GeographicCoordinate>
	{

		public GeographicGeocentricTranslation([NotNull] ISpheroid<double> spheroidFrom, Vector3 delta, [NotNull] ISpheroid<double> spheroidTo)
			: this(new GeographicGeocentricTransformation(spheroidFrom), delta, new GeocentricGeographicTransformation(spheroidTo)) { }

		private GeographicGeocentricTranslation([NotNull] GeographicGeocentricTransformation geogGeoc, Vector3 delta, [NotNull] GeocentricGeographicTransformation geocGeog) {
			_geogGeoc = geogGeoc;
			_geocGeog = geocGeog;
			_delta = delta;
		}
 
		private readonly GeographicGeocentricTransformation _geogGeoc;
		private readonly GeocentricGeographicTransformation _geocGeog;
		private readonly Vector3 _delta;

		public GeographicCoordinate TransformValue(GeographicCoordinate value) {
			return _geocGeog.TransformValue2D(_geogGeoc.TransformValue(value).Add(_delta));
		}

		private void TransformValue(ref GeographicCoordinate value) {
			value = _geocGeog.TransformValue2D(_geogGeoc.TransformValue(value).Add(_delta));
		}

		public IEnumerable<GeographicCoordinate> TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public void TransformValues([NotNull] GeographicCoordinate[] values) {
			for (var i = 0; i < values.Length; i++)
				TransformValue(ref values[i]);
		}

		public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
			return _geocGeog.TransformValue(_geogGeoc.TransformValue(value).Add(_delta));
		}

		private void TransformValue(ref GeographicHeightCoordinate value) {
			value = _geocGeog.TransformValue(_geogGeoc.TransformValue(value).Add(_delta));
		}

		public IEnumerable<GeographicHeightCoordinate> TransformValues([NotNull] IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}

		public void TransformValues([NotNull] GeographicHeightCoordinate[] values) {
			for (var i = 0; i < values.Length; i++)
				TransformValue(ref values[i]);
		}

		IEnumerable<GeographicHeightCoordinate> ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue3D);
		}

		public GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
			return _geocGeog.TransformValue(_geogGeoc.TransformValue(value).Add(_delta));
		}

		GeographicHeightCoordinate ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.TransformValue(GeographicCoordinate value) {
			return TransformValue3D(value);
		}

		IEnumerable<GeographicCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.TransformValues([NotNull] IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue2D);
		}

		public GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
			return _geocGeog.TransformValue2D(_geogGeoc.TransformValue(value).Add(_delta));
		}

		GeographicCoordinate ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.TransformValue(GeographicHeightCoordinate value) {
			return TransformValue2D(value);
		}

		public ITransformation GetInverse() {
			if(!HasInverse)
				throw new InvalidOperationException("No inverse.");
			return new GeographicGeocentricTranslation(
				_geocGeog.GetInverse(),
				_delta.GetNegative(),
				_geogGeoc.GetInverse()
			);
		}

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

		public bool HasInverse {
			get {
				return _geogGeoc.HasInverse
					&& _geocGeog.HasInverse;
			}
		}
	}
}
