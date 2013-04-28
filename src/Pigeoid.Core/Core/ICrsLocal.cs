using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.Unit;

namespace Pigeoid
{
    /// <summary>
    /// A local coordinate reference system.
    /// </summary>
    [ContractClass(typeof(ICrsLocalCodeContracts))]
    public interface ICrsLocal : ICrs
    {
        /// <summary>
        /// The datum the coordinate reference system is based on.
        /// </summary>
        IDatum Datum { get; }

        /// <summary>
        /// The unit of measure used by this CRS.
        /// </summary>
        IUnit Unit { get; }

        /// <summary>
        /// The axes for the projection.
        /// </summary>
        IList<IAxis> Axes { get; }

    }

    [ContractClassFor(typeof(ICrsLocal))]
    internal abstract class ICrsLocalCodeContracts : ICrsLocal
    {

        public IDatum Datum {
            get {
                Contract.Ensures(Contract.Result<IDatum>() != null);
                throw new NotImplementedException();
            }
        }

        public IUnit Unit {
            get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                throw new NotImplementedException();
            }
        }

        public IList<IAxis> Axes {
            get {
                Contract.Ensures(Contract.Result<IList<IAxis>>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract IAuthorityTag Authority { get; }
    }
}
