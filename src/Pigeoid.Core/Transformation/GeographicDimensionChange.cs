using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
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
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        public GeographicCoordinate TransformValue(GeographicHeightCoordinate value) {
            return new GeographicCoordinate(value.Latitude, value.Longitude);
        }

        public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<IEnumerable<GeographicCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        bool ITransformation.HasInverse {
            get { return true; }
        }

        ITransformation<GeographicHeightCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicHeightCoordinate>.GetInverse() {
            return this;
        }

        ITransformation<GeographicCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicCoordinate>.GetInverse() {
            return this;
        }

        ITransformation ITransformation.GetInverse() {
            return this;
        }

    }

}
