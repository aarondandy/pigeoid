using Pigeoid.Core;
using Pigeoid.Epsg;

namespace Pigeoid.CoordinateOpFullTester
{
    public class CoordinateOpTestCase
    {

        public CoordinateOpTestCase(EpsgCrs from, EpsgCrs to) {
            From = from;
            To = to;
        }

        public EpsgCrs From { get; private set; }
        public EpsgCrs To { get; private set; }

        public IGeographicMbr IntersectingArea {
            get {
                return From.Area.Intersection(To.Area);
            }
        }

        public override string ToString() {
            return string.Format("{0} to {1}", From, To);
        }

    }
}
