using Pigeoid.CoordinateOperation;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOpFullTester
{
    public class CoordOpTransformPath
    {
        public ICrs From { get; set; }
        public ICrs To { get; set; }
        public ICoordinateOperationCrsPathInfo Path { get; set; }
        public ITransformation Transformation { get; set; }
    }
}
