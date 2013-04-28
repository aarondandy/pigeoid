using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur.Contracts;

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

    }
}
