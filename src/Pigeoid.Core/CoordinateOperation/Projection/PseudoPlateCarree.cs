using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class PseudoPlateCarree : ITransformation<GeographicCoordinate, Point2>
    {

        private class Inverted : ITransformation<Point2,GeographicCoordinate>
        {

            public static readonly Inverted DefaultReverse = new Inverted();

            private Inverted() { }

            public ITransformation<GeographicCoordinate, Point2> GetInverse() {
                return Default;
            }

            public GeographicCoordinate TransformValue(Point2 value) {
                return new GeographicCoordinate(value.X, value.Y);
            }

            public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<Point2> values) {
                return values.Select(TransformValue);
            }

            ITransformation ITransformation.GetInverse() {
                return GetInverse();
            }

            bool ITransformation.HasInverse {
                [Pure] get { return true; }
            }
        }

        public static readonly PseudoPlateCarree Default = new PseudoPlateCarree();

        private PseudoPlateCarree() { }

        public ITransformation<Point2, GeographicCoordinate> GetInverse() {
            return Inverted.DefaultReverse;
        }

        public Point2 TransformValue(GeographicCoordinate value) {
            return new Point2(value.Latitude, value.Longitude);
        }

        public IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values) {
            return values.Select(TransformValue);
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        bool ITransformation.HasInverse {
            [Pure] get { return true; }
        }
    }
}
