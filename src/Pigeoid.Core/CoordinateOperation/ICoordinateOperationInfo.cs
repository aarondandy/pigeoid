using System;
using System.Diagnostics.Contracts;

namespace Pigeoid.CoordinateOperation
{

    [ContractClass(typeof(ICoordinateOperationInfoCodeContracts))]
    public interface ICoordinateOperationInfo
    {

        /// <summary>
        /// The name of the operation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines if this operation has an inverse.
        /// </summary>
        bool HasInverse { [Pure] get; }

        /// <summary>
        /// Gets the inverse of this operation information if one exists.
        /// </summary>
        /// <returns>An operation.</returns>
        ICoordinateOperationInfo GetInverse();

        /// <summary>
        /// Flag is set when this represents the inverse of a well defined coordinate operation.
        /// </summary>
        bool IsInverseOfDefinition { [Pure] get; }

    }

    [ContractClassFor(typeof(ICoordinateOperationInfo))]
    internal abstract class ICoordinateOperationInfoCodeContracts : ICoordinateOperationInfo
    {

        public string Name {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract bool HasInverse { [Pure] get; }

        public ICoordinateOperationInfo GetInverse() {
            Contract.Requires(HasInverse);
            Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
            throw new NotImplementedException();
        }

        public abstract bool IsInverseOfDefinition { get; }
    }
}
