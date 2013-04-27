using System;
using System.Diagnostics.Contracts;

namespace Pigeoid.Contracts
{
    /// <summary>
    /// A fitted coordinate reference system.
    /// </summary>
    [ContractClass(typeof(ICrsFittedCodeContract))]
    public interface ICrsFitted : ICrs
    {
        /// <summary>
        /// The base CRS of this fitted CRS.
        /// </summary>
        ICrs BaseCrs { get; }

        /// <summary>
        /// The operation which converts from this CRS to the base CRS.
        /// </summary>
        ICoordinateOperationInfo ToBaseOperation { get; }
    }

    [ContractClassFor(typeof(ICrsFitted))]
    internal abstract class ICrsFittedCodeContract: ICrsFitted
    {

        public ICrs BaseCrs {
            get {
                Contract.Ensures(Contract.Result<ICrs>() != null);
                throw new NotImplementedException();
            }
        }

        public ICoordinateOperationInfo ToBaseOperation {
            get {
                Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract IAuthorityTag Authority { get; }
    }

}
