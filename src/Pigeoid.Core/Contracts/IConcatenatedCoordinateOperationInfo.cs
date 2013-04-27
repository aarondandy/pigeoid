using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Pigeoid.Contracts
{
    [ContractClass(typeof(IConcatenatedCoordinateOperationInfoCodeContracts))]
    public interface IConcatenatedCoordinateOperationInfo : ICoordinateOperationInfo
    {
        IEnumerable<ICoordinateOperationInfo> Steps { get; }
    }

    [ContractClassFor(typeof(IConcatenatedCoordinateOperationInfo))]
    internal abstract class IConcatenatedCoordinateOperationInfoCodeContracts : IConcatenatedCoordinateOperationInfo
    {

        public IEnumerable<ICoordinateOperationInfo> Steps {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>() != null);
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>().Count() > 0);
                throw new NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract bool HasInverse { get; }

        public abstract ICoordinateOperationInfo GetInverse();

        public abstract bool IsInverseOfDefinition { get; }
    }
}
