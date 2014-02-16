using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class MadridEd50Polynomial :
        ITransformation<GeographicCoordinate>,
        ITransformation<GeographicHeightCoordinate, GeographicCoordinate>
    {

        public double A0 { get; private set; }
        public double A1 { get; private set; }
        public double A2 { get; private set; }
        public double A3 { get; private set; }

        public double B00 { get; private set; }
        public double B0 { get; private set; }
        public double B1 { get; private set; }
        public double B2 { get; private set; }
        public double B3 { get; private set; }

        private readonly double _b0Total;

        public MadridEd50Polynomial(
            double a0,
            double a1,
            double a2,
            double a3,
            double b00,
            double b0,
            double b1,
            double b2,
            double b3
        ) {
            A0 = a0;
            A1 = a1;
            A2 = a2;
            A3 = a3;
            B00 = b00;
            B0 = b0;
            B1 = b1;
            B2 = b2;
            B3 = b3;
            _b0Total = B00 + B0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Coordinates in decimal degrees.</param>
        /// <returns>result in decimal degrees.</returns>
        public GeographicCoordinate TransformValue(GeographicHeightCoordinate value) {
            var dLat = A0 + (A1 * value.Latitude) + (A2 * value.Longitude) + (A3 * value.Height);
            var dLon = _b0Total + (B1 * value.Latitude) + (B2 * value.Longitude) + (B3 * value.Height);
            // be sure to convert the delta from arc-seconds to degrees
            return new GeographicCoordinate(value.Latitude + (dLat / 3600.0), value.Longitude + (dLon / 3600.0));
        }

        public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        public GeographicCoordinate TransformValue(GeographicCoordinate value) {
            return TransformValue(new GeographicHeightCoordinate(value.Latitude, value.Longitude));
        }

        public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        public void TransformValues(GeographicCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }

        ITransformation<GeographicCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicCoordinate>.GetInverse() {
            throw new NoInverseException();
        }

        ITransformation ITransformation.GetInverse() {
            throw new NoInverseException();
        }

        ITransformation<GeographicCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.GetInverse() {
            throw new NoInverseException();
        }

        ITransformation<GeographicCoordinate> ITransformation<GeographicCoordinate>.GetInverse() {
            throw new NoInverseException();
        }

        public bool HasInverse {
            [Pure] get { return false; }
        }


        public Type[] GetInputTypes() {
            return new[] {typeof (GeographicHeightCoordinate), typeof (GeographicCoordinate)};
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(GeographicHeightCoordinate)
                || inputType == typeof(GeographicCoordinate)
                ? new[] { typeof(GeographicCoordinate) }
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
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }
    }
}
