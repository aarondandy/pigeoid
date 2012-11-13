using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class AngularElementTransformation :
		ITransformation<Vector2>,
		ITransformation<Vector3>,
		ITransformation<GeographicCoordinate>,
		ITransformation<GeographicHeightCoordinate>
	{

		private readonly ITransformation<double> _core;

		public AngularElementTransformation([NotNull] ITransformation<double> core) {
			if(null == core)
				throw new ArgumentNullException("core");

			_core = core;
		}

		public bool HasInverse {
			get { return _core.HasInverse; }
		}

		public AngularElementTransformation GetInverse() {
			if(!HasInverse)
				throw new InvalidOperationException("No inverse.");
			return new AngularElementTransformation(_core.GetInverse());
		}

		ITransformation<Vector2> ITransformation<Vector2>.GetInverse(){
			return GetInverse();
		}
		ITransformation<Vector2, Vector2> ITransformation<Vector2, Vector2>.GetInverse() {
			return GetInverse();
		}
		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}
		ITransformation<Vector3> ITransformation<Vector3>.GetInverse() {
			return GetInverse();
		}
		ITransformation<Vector3, Vector3> ITransformation<Vector3, Vector3>.GetInverse() {
			return GetInverse();
		}
		ITransformation<GeographicCoordinate> ITransformation<GeographicCoordinate>.GetInverse() {
			return GetInverse();
		}
		ITransformation<GeographicCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicCoordinate>.GetInverse() {
			return GetInverse();
		}
		ITransformation<GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate>.GetInverse() {
			return GetInverse();
		}
		ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>.GetInverse() {
			return GetInverse();
		}

		public Vector2 TransformValue(Vector2 value) {
			return new Vector2(
				_core.TransformValue(value.X),
				_core.TransformValue(value.Y));
		}
		public Vector3 TransformValue(Vector3 value) {
			return new Vector3(
				_core.TransformValue(value.X),
				_core.TransformValue(value.Y),
				_core.TransformValue(value.Z));
		}
		public GeographicCoordinate TransformValue(GeographicCoordinate value) {
			return new GeographicCoordinate(
				_core.TransformValue(value.Latitude),
				_core.TransformValue(value.Longitude));
		}
		public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
			return new GeographicHeightCoordinate(
				_core.TransformValue(value.Latitude),
				_core.TransformValue(value.Longitude),
				value.Height);
		}

		public void TransformValues([NotNull] Vector2[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}
		public void TransformValues([NotNull] Vector3[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}
		public void TransformValues([NotNull] GeographicCoordinate[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}
		public void TransformValues([NotNull] GeographicHeightCoordinate[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}

		public IEnumerable<Vector2> TransformValues([NotNull] IEnumerable<Vector2> values){
			return values.Select(TransformValue);
		}
		public IEnumerable<Vector3> TransformValues([NotNull] IEnumerable<Vector3> values) {
			return values.Select(TransformValue);
		}
		public IEnumerable<GeographicCoordinate> TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}
		public IEnumerable<GeographicHeightCoordinate> TransformValues([NotNull] IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}

	}
}
