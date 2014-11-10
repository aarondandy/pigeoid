using DotSpatial.Projections;
using Pigeoid.CoordinateOperation;
using Pigeoid.Core;
using Pigeoid.Interop.Proj4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pigeoid.Proj4ComparisonTests
{
    public class TestCase
    {

        public ICrs Source { get; set; }
        public ICrs Target { get; set; }

        public ProjectionInfo Proj4Source { get { return Proj4Crs.CreateProjection(Source); } }

        public ProjectionInfo Proj4Target { get { return Proj4Crs.CreateProjection(Target); } } 

        public IGeographicMbr Area { get; set; }
        public List<GeographicCoordinate> InputWgs84Coordinates { get; set; }
        public List<object> InputCoordinates { get; set; }

        public override string ToString() {
            return String.Format("{0} to {1}", Source, Target);
        }

    }
}
