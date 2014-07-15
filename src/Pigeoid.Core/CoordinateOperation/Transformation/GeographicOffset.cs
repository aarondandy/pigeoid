using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class GeographicOffset :
        ITransformation<GeographicCoordinate>,
        ITransformation<GeographicHeightCoordinate>,
        ITransformation<GeographicCoordinate, GeographicHeightCoordinate>,
        ITransformation<GeographicHeightCoordinate, GeographicCoordinate>
    {

        private readonly GeographicHeightCoordinate _delta;

        public GeographicHeightCoordinate Delta {
            get { return _delta; }
        }

        public GeographicOffset(double deltaLat, double deltaLon)
            : this(deltaLat, deltaLon, 0) { }

        public GeographicOffset(double deltaLat, double deltaLon, double deltaHeight)
            : this(new GeographicHeightCoordinate(deltaLat, deltaLon, deltaHeight)) { }

        public GeographicOffset(GeographicHeightCoordinate delta) {
            _delta = delta;
        }

        public GeographicOffset(GeographicCoordinate d)
            : this(d.Latitude, d.Longitude) { }

        public void TransformValues(GeographicCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                TransformValue(ref values[i]);
        }

        private void TransformValue(ref GeographicCoordinate value) {
            value = new GeographicCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude);
        }

        public GeographicCoordinate TransformValue(GeographicCoordinate value) {
            return new GeographicCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude);
        }

        public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return new GeographicHeightCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude, value.Height + Delta.Height);
        }

        private void TransformValue(ref GeographicHeightCoordinate value) {
            value = new GeographicHeightCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude, value.Height + Delta.Height);
        }

        public void TransformValues(GeographicHeightCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                TransformValue(ref values[i]);
        }

        public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        ITransformation<GeographicHeightCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        GeographicHeightCoordinate ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.TransformValue(GeographicCoordinate value) {
            return new GeographicHeightCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude, Delta.Height);
        }

        IEnumerable<GeographicHeightCoordinate> ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(((ITransformation<GeographicCoordinate, GeographicHeightCoordinate>)this).TransformValue);
        }

        ITransformation<GeographicCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.GetInverse() {
            return GetInverse();
        }

        GeographicCoordinate ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.TransformValue(GeographicHeightCoordinate value) {
            return new GeographicCoordinate(value.Latitude + Delta.Latitude, value.Longitude + Delta.Longitude);
        }

        IEnumerable<GeographicCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicCoordinate>>() != null);
            return values.Select(((ITransformation<GeographicHeightCoordinate, GeographicCoordinate>)this).TransformValue);
        }

        ITransformation<GeographicCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicCoordinate>.GetInverse() {
            return GetInverse();
        }

        public bool HasInverse {
            [Pure] get { return true; }
        }

        public GeographicOffset GetInverse() {
            Contract.Ensures(Contract.Result<GeographicOffset>() != null);
            return new GeographicOffset(new GeographicHeightCoordinate(-Delta.Latitude, -Delta.Longitude, -Delta.Height));
        }

        ITransformation<GeographicCoordinate> ITransformation<GeographicCoordinate>.GetInverse() {
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

        public Type[] GetInputTypes() {
            return new[] {typeof (GeographicHeightCoordinate), typeof (GeographicCoordinate)};
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(GeographicHeightCoordinate)
                || inputType == typeof(GeographicCoordinate)
                ? new[]{inputType}
                : ArrayUtil<Type>.Empty;
        }

        public object TransformValue(object value) {
            if (value is GeographicHeightCoordinate)
                return TransformValue((GeographicHeightCoordinate) value);
            if (value is GeographicCoordinate)
                return TransformValue((GeographicCoordinate) value);
            throw new InvalidOperationException();
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }

    }
}
