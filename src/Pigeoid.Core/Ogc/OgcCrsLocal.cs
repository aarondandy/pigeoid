using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Unit;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A local CRS.
    /// </summary>
    public class OgcCrsLocal : OgcNamedAuthorityBoundEntity, ICrsLocal
    {

        /// <summary>
        /// Constructs a new local CRS.
        /// </summary>
        /// <param name="name">The CRS name.</param>
        /// <param name="datum">The datum the CRS is based on.</param>
        /// <param name="unit">The unit for the CRS.</param>
        /// <param name="axes">The axes of the CRS.</param>
        /// <param name="authority">The authority.</param>
        public OgcCrsLocal(
            string name,
            IDatum datum,
            IUnit unit,
            IEnumerable<IAxis> axes,
            IAuthorityTag authority
        )
            : base(name, authority) {
            if (datum == null) throw new ArgumentNullException("datum");
            if (unit == null) throw new ArgumentNullException("unit");
            Contract.Requires(name != null);
            Datum = datum;
            Unit = unit;
            Axes = Array.AsReadOnly(null == axes ? new IAxis[0] : axes.ToArray());
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Datum != null);
            Contract.Invariant(Unit != null);
            Contract.Invariant(Axes != null);
        }

        /// <inheritdoc/>
        public IDatum Datum { get; private set; }

        /// <inheritdoc/>
        public IUnit Unit { get; private set; }

        /// <inheritdoc/>
        public IList<IAxis> Axes { get; private set; }

    }
}
