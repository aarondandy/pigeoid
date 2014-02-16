using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class VerticalOffset :
        ITransformation<double>,
        ITransformation<GeographicHeightCoordinate>
    {

        private readonly double _offset;

        public VerticalOffset(double offset) {
            _offset = offset;
        }

        public double Offset { get { return _offset; } }

        public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return new GeographicHeightCoordinate(value.Latitude, value.Longitude, value.Height + _offset);
        }

        public void TransformValues(GeographicHeightCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }

        public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        public double TransformValue(double value) {
            return value + _offset;
        }

        public void TransformValues(double[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] += _offset;
        }

        public IEnumerable<double> TransformValues(IEnumerable<double> values) {
            Contract.Ensures(Contract.Result<IEnumerable<double>>() != null);
            return values.Select(TransformValue);
        }

        public VerticalOffset GetInverse() {
            Contract.Ensures(Contract.Result<VerticalOffset>() != null);
            return new VerticalOffset(-_offset);
        }

        ITransformation<double, double> ITransformation<double, double>.GetInverse() {
            return GetInverse();
        }

        ITransformation<double> ITransformation<double>.GetInverse() {
            return GetInverse();
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        public bool HasInverse {
            [Pure] get { return true; }
        }

        public Type[] GetInputTypes() {
            return new[] {typeof(GeographicHeightCoordinate), typeof(double)};
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(GeographicHeightCoordinate)
                || inputType == typeof(double)
                ? new[] { inputType }
                : ArrayUtil<Type>.Empty;
        }

        public object TransformValue(object value) {
            if (value is GeographicHeightCoordinate)
                return TransformValue((GeographicHeightCoordinate) value);
            if (value is double)
                return TransformValue((double) value);
            throw new InvalidOperationException();
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }
    }
}
