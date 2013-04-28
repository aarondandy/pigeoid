using System;
using System.Diagnostics.Contracts;
using Pigeoid.Unit;

namespace Pigeoid
{
    /// <summary>
    /// A vertical coordinate reference system.
    /// </summary>
    [ContractClass(typeof(ICrsVerticalCodeContracts))]
    public interface ICrsVertical : ICrs
    {
        /// <summary>
        /// The datum the CRS is based on.
        /// </summary>
        IDatum Datum { get; }

        /// <summary>
        /// The vertical unit of measure for this CRS.
        /// </summary>
        IUnit Unit { get; }

        /// <summary>
        /// The axis for this CRS.
        /// </summary>
        IAxis Axis { get; }
    }

    [ContractClassFor(typeof(ICrsVertical))]
    internal abstract class ICrsVerticalCodeContracts : ICrsVertical
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

        public IAxis Axis {
            get {
                Contract.Ensures(Contract.Result<IAxis>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract IAuthorityTag Authority { get; }
    }
}
