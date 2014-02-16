using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class GeographicDimensionChange :
        ITransformation<GeographicCoordinate, GeographicHeightCoordinate>,
        ITransformation<GeographicHeightCoordinate, GeographicCoordinate>
    {

        private readonly double _defaultHeight;

        public GeographicDimensionChange() : this(0) { }

        public GeographicDimensionChange(double defaultHeight) {
            _defaultHeight = defaultHeight;
        }

        public double DefaultHeight { get { return _defaultHeight; } }

        public GeographicHeightCoordinate TransformValue(GeographicCoordinate value) {
            return new GeographicHeightCoordinate(value.Latitude, value.Longitude, _defaultHeight);
        }

        public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        public GeographicCoordinate TransformValue(GeographicHeightCoordinate value) {
            return (GeographicCoordinate)value;
        }

        public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        bool ITransformation.HasInverse { get { return true; } }

        ITransformation<GeographicHeightCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.GetInverse() {
            return this;
        }

        ITransformation<GeographicCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.GetInverse() {
            return this;
        }

        ITransformation ITransformation.GetInverse() {
            return this;
        }

        public Type[] GetInputTypes() {
            return new[] { typeof(GeographicHeightCoordinate), typeof(GeographicCoordinate) };
        }

        public Type[] GetOutputTypes(Type inputType) {
            if (inputType == typeof (GeographicHeightCoordinate))
                return new[] {typeof(GeographicCoordinate)};
            if (inputType == typeof (GeographicCoordinate))
                return new[] {typeof(GeographicHeightCoordinate)};
            return ArrayUtil<Type>.Empty;
        }

        public object TransformValue(object value) {
            if (value is GeographicHeightCoordinate)
                return TransformValue((GeographicHeightCoordinate)value);
            if (value is GeographicCoordinate)
                return TransformValue((GeographicCoordinate)value);
            throw new InvalidOperationException();
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            throw new NotImplementedException();
        }
    }

}
