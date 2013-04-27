using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.Contracts
{
    /// <summary>
    /// A geodetic coordinate reference system.
    /// </summary>
    [ContractClass(typeof(ICrsGeodeticCodeContracts))]
    public interface ICrsGeodetic : ICrs
    {
        /// <summary>
        /// The datum this coordinate reference system is based on.
        /// </summary>
        IDatumGeodetic Datum { get; }

        /// <summary>
        /// The unit of measure for this CRS.
        /// </summary>
        IUnit Unit { get; }

        /// <summary>
        /// Gets a collection of axes for this CRS.
        /// </summary>
        IList<IAxis> Axes { get; }
    }
    [ContractClassFor(typeof(ICrsGeodetic))]
    internal abstract class ICrsGeodeticCodeContracts : ICrsGeodetic
    {

        public IDatumGeodetic Datum {
            get {
                Contract.Ensures(Contract.Result<IDatumGeodetic>() != null);
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
