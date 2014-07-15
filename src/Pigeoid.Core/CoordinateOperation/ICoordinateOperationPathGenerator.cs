using System;
using System.Collections.Generic;
namespace Pigeoid.CoordinateOperation
{
    public interface ICoordinateOperationPathGenerator<in TItem>
    {
        IEnumerable<ICoordinateOperationCrsPathInfo> Generate(TItem from, TItem to);
    }
}
