using System.Collections.Generic;

namespace Pigeoid.Contracts
{
    public interface IConcatenatedCoordinateOperationInfo : ICoordinateOperationInfo
    {

        IEnumerable<ICoordinateOperationInfo> Steps { get; }

    }
}
