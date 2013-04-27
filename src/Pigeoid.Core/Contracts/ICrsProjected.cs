using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.Contracts
{
    /// <summary>
    /// A projected coordinate reference system.
    /// </summary>
    [ContractClass(typeof(ICrsProjectCodeContracts))]
    public interface ICrsProjected : ICrsGeodetic
    {
        /// <summary>
        /// The CRS this projected CRS is based on.
        /// </summary>
        ICrsGeodetic BaseCrs { get; }
        /// <summary>
        /// The projection operation.
        /// </summary>
        ICoordinateOperationInfo Projection { get; }

    }

    [ContractClassFor(typeof(ICrsProjected))]
    internal abstract class ICrsProjectCodeContracts : ICrsProjected
    {

        public ICrsGeodetic BaseCrs {
            get {
                Contract.Ensures(Contract.Result<ICrsGeodetic>() != null);
                throw new NotImplementedException();
            }
        }

        public ICoordinateOperationInfo Projection {
            get {
                Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract IDatumGeodetic Datum { get; }

        public abstract IUnit Unit { get; }

        public abstract IList<IAxis> Axes { get; }

        public abstract string Name { get; }

        public abstract IAuthorityTag Authority { get; }
    }

}
