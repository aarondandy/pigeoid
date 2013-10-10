using Pigeoid.CoordinateOperation;
using Pigeoid.Core;

namespace Pigeoid.CoordinateOpFullTester
{
    public class CoordOpTestCase
    {

        public ICrs From { get; set; }
        public ICrs To { get; set; }
        public IGeographicMbr Area { get; set; }
        public ICoordinateOperationCrsPathInfo Path { get; set; }

        public override string ToString() {
            return string.Format("{0} to {1}", From, To);
        }

    }
}
