using System.Collections.Generic;

namespace Pigeoid.CoordinateOperation
{
    public interface ICoordinateOperationCrsPathInfo
    {
        /// <summary>
        /// The collection of coordinate reference systems that make up the start, end, and intermediary states of the operation path.
        /// </summary>
        /// <remarks>
        /// There should always be one more CRS item than operation item.
        /// </remarks>
        IEnumerable<ICrs> CoordinateReferenceSystems { get; }

        /// <summary>
        /// The operations that convert from the previous and next CRS.
        /// </summary>
        IEnumerable<ICoordinateOperationInfo> CoordinateOperations { get; }

    }
}
