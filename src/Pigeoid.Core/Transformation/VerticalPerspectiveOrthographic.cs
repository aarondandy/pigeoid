using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
    public class VerticalPerspectiveOrthographic :
        ITransformation<GeographicHeightCoordinate, Point2>,
        ITransformation<GeographicCoordinate, Point2>
    {

        public Point2 TransformValue(GeographicHeightCoordinate value) {
            throw new NotImplementedException();
        }

        public IEnumerable<Point2> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }
        public Point2 TransformValue(GeographicCoordinate value) {
            return TransformValue(new GeographicHeightCoordinate(value.Latitude, value.Longitude, 0));
        }
        public IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }

        ITransformation<Point2, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, Point2>.GetInverse() {
            throw new NoInverseException();
        }
        ITransformation<Point2, GeographicCoordinate> ITransformation<GeographicCoordinate, Point2>.GetInverse() {
            throw new NoInverseException();
        }
        ITransformation ITransformation.GetInverse() {
            throw new NoInverseException();
        }
        bool ITransformation.HasInverse {
            get { return false; }
        }


    }
}
