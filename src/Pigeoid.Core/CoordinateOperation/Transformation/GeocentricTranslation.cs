using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    /// <summary>
    /// A geocentric translation.
    /// </summary>
    public class GeocentricTranslation : ITransformation<Point3>
    {

        public readonly Vector3 Delta;

        /// <summary>
        /// Constructs a new geocentric translation operation.
        /// </summary>
        /// <param name="delta"></param>
        public GeocentricTranslation(Vector3 delta) {
            Delta = delta;
        }

        public Point3 TransformValue(Point3 point) {
            return point.Add(Delta);
        }

        public IEnumerable<Point3> TransformValues(IEnumerable<Point3> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point3>>() != null);
            return values.Select(Delta.Add);
        }

        public void TransformValues(Point3[] values) {
            for (int i = 0; i < values.Length; i++)
                TransformValue(ref values[i]);
        }

        private void TransformValue(ref Point3 point) {
            point = point.Add(Delta);
        }

        public bool HasInverse {
            [Pure] get { return true; }
        }

        ITransformation<Point3, Point3> ITransformation<Point3, Point3>.GetInverse() {
            return GetInverse();
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        public ITransformation<Point3> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point3>>() != null);
            return new GeocentricTranslation(Delta.GetNegative());
        }

        public Type[] GetInputTypes() {
            return new[] { typeof(Point3) };
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(Point3)
                ? new[] { typeof(Point3) }
                : ArrayUtil<Type>.Empty;
        }

        public object TransformValue(object value) {
            return TransformValue((Point3)value);
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }
    }
}
