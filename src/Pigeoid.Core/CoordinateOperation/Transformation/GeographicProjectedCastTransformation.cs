using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class GeographicProjectedCastTransformation :
        ITransformation<Point2, GeographicCoordinate>,
        ITransformation<Point2, GeographicHeightCoordinate>,
        ITransformation<GeographicCoordinate, Point2>,
        ITransformation<GeographicHeightCoordinate, Point2>
    {
        bool ITransformation.HasInverse {
            [Pure] get { return true; }
        }
        ITransformation<Point2, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, Point2>.GetInverse() {
            return this;
        }
        ITransformation ITransformation.GetInverse() {
            return this;
        }
        ITransformation<Point2, GeographicCoordinate> ITransformation<GeographicCoordinate, Point2>.GetInverse() {
            return this;
        }
        ITransformation<GeographicHeightCoordinate, Point2> ITransformation<Point2, GeographicHeightCoordinate>.GetInverse() {
            return this;
        }
        ITransformation<GeographicCoordinate, Point2> ITransformation<Point2, GeographicCoordinate>.GetInverse() {
            return this;
        }

        public Point2 TransformValue(GeographicHeightCoordinate value) {
            return new Point2(value.Longitude, value.Latitude);
        }
        public IEnumerable<Point2> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }
        public Point2 TransformValue(GeographicCoordinate value) {
            return new Point2(value.Longitude, value.Latitude);
        }
        public IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }

        public GeographicHeightCoordinate TransformValue3D(Point2 value) {
            return new GeographicHeightCoordinate(value.Y, value.X);
        }
        GeographicHeightCoordinate ITransformation<Point2, GeographicHeightCoordinate>.TransformValue(Point2 value) {
            return TransformValue3D(value);
        }
        public IEnumerable<GeographicHeightCoordinate> TransformValues3D(IEnumerable<Point2> values) {
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue3D);
        }
        IEnumerable<GeographicHeightCoordinate> ITransformation<Point2, GeographicHeightCoordinate>.TransformValues(IEnumerable<Point2> values) {
            return TransformValues3D(values);
        }

        public GeographicCoordinate TransformValue(Point2 value) {
            return new GeographicCoordinate(value.Y, value.X);
        }
        public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<Point2> values) {
            return values.Select(TransformValue);
        }


        public Type[] GetInputTypes() {
            return new[] {
                typeof(Point2),
                typeof(GeographicHeightCoordinate),
                typeof(GeographicCoordinate)
            };
        }

        public Type[] GetOutputTypes(Type inputType) {
            if (inputType == typeof (Point2))
                return new[] { typeof(GeographicHeightCoordinate), typeof(GeographicCoordinate) };
            return inputType == typeof(GeographicHeightCoordinate)
                || inputType == typeof(GeographicCoordinate)
                ? new[] { typeof(Point3) }
                : ArrayUtil<Type>.Empty;
        }

        public object TransformValue(object value) {
            if (value is Point2)
                return TransformValue((Point2) value);
            if (value is GeographicCoordinate)
                return TransformValue((GeographicCoordinate) value);
            if (value is GeographicHeightCoordinate)
                return TransformValue((GeographicHeightCoordinate) value);
            throw new InvalidOperationException();

        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }
    }
}
