using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.CoordinateOperation
{

    [ContractClass(typeof(CoordinateOperationCrsPathInfoContracts))]
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

        /// <summary>
        /// The first CRS of the path.
        /// </summary>
        ICrs From { get; }

        /// <summary>
        /// The last CRS of the path.
        /// </summary>
        ICrs To { get; }

    }

    [ContractClassFor(typeof(ICoordinateOperationCrsPathInfo))]
    internal abstract class CoordinateOperationCrsPathInfoContracts : ICoordinateOperationCrsPathInfo
    {
        public IEnumerable<ICrs> CoordinateReferenceSystems {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICrs>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICrs>>(), x => x != null));
                throw new NotImplementedException();
            }
        }

        public IEnumerable<ICoordinateOperationInfo> CoordinateOperations {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICoordinateOperationInfo>>(), x => x != null));
                throw new NotImplementedException();
            }
        }

        public ICrs From {
            get {
                Contract.Ensures(Contract.Result<ICrs>() != null);
                throw new NotImplementedException();
            }
        }

        public ICrs To {
            get {
                Contract.Ensures(Contract.Result<ICrs>() != null);
                throw new NotImplementedException();
            }
        }
    }

}
