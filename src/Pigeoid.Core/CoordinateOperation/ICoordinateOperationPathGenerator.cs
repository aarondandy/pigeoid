using System;
namespace Pigeoid.CoordinateOperation
{
    public interface ICoordinateOperationPathGenerator<in TItem>
    {
        [Obsolete]
        ICoordinateOperationCrsPathInfo Generate(TItem from, TItem to);
    }
}
