using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class LinearElementTransformation :
        ITransformation<Point2>,
        ITransformation<Point3>,
        ITransformation<Vector2>,
        ITransformation<Vector3>,
        ITransformation<GeographicHeightCoordinate>
    {

        private readonly ITransformation<double> _core;

        public LinearElementTransformation(ITransformation<double> core) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            _core = core;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_core != null);
        }

        public bool HasInverse {
            [Pure] get { return _core.HasInverse; }
        }

        public LinearElementTransformation GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<LinearElementTransformation>() != null);
            return new LinearElementTransformation(_core.GetInverse());
        }

        ITransformation<Point2> ITransformation<Point2>.GetInverse() {
            return GetInverse();
        }

        ITransformation<Point2, Point2> ITransformation<Point2, Point2>.GetInverse() {
            return GetInverse();
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        ITransformation<Point3> ITransformation<Point3>.GetInverse() {
            return GetInverse();
        }

        ITransformation<Point3, Point3> ITransformation<Point3, Point3>.GetInverse() {
            return GetInverse();
        }

        ITransformation<Vector2> ITransformation<Vector2>.GetInverse() {
            return GetInverse();
        }

        ITransformation<Vector2, Vector2> ITransformation<Vector2, Vector2>.GetInverse() {
            return GetInverse();
        }

        ITransformation<Vector3> ITransformation<Vector3>.GetInverse() {
            return GetInverse();
        }

        ITransformation<Vector3, Vector3> ITransformation<Vector3, Vector3>.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        public Point2 TransformValue(Point2 value) {
            return new Point2(
                _core.TransformValue(value.X),
                _core.TransformValue(value.Y));
        }
        public Point3 TransformValue(Point3 value) {
            return new Point3(
                _core.TransformValue(value.X),
                _core.TransformValue(value.Y),
                _core.TransformValue(value.Z));
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
        public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return new GeographicHeightCoordinate(
                value.Latitude,
                value.Longitude,
                _core.TransformValue(value.Height));
        }

        public void TransformValues(Point2[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }
        public void TransformValues(Point3[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }
        public void TransformValues(Vector2[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }
        public void TransformValues(Vector3[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }
        public void TransformValues(GeographicHeightCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }

        public IEnumerable<Point2> TransformValues(IEnumerable<Point2> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }
        public IEnumerable<Point3> TransformValues(IEnumerable<Point3> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point3>>() != null);
            return values.Select(TransformValue);
        }
        public IEnumerable<Vector2> TransformValues(IEnumerable<Vector2> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Vector2>>() != null);
            return values.Select(TransformValue);
        }
        public IEnumerable<Vector3> TransformValues(IEnumerable<Vector3> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Vector3>>() != null);
            return values.Select(TransformValue);
        }
        public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

    }
}
