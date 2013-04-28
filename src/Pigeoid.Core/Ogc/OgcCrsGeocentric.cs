using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Unit;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A geocentric CRS.
    /// </summary>
    public class OgcCrsGeocentric : OgcNamedAuthorityBoundEntity, ICrsGeocentric
    {

        /// <summary>
        /// Constructs a new geocentric CRS.
        /// </summary>
        /// <param name="name">The name of the CRS.</param>
        /// <param name="datum">The datum the CRS is based on.</param>
        /// <param name="linearUnit">The linear UoM to use for the CRS.</param>
        /// <param name="axes">The axes which define the space.</param>
        /// <param name="authority">The authority.</param>
        public OgcCrsGeocentric(
            string name,
            IDatumGeodetic datum,
            IUnit linearUnit,
            IEnumerable<IAxis> axes,
            IAuthorityTag authority
        )
            : base(name, authority)
        {
            if (null == datum) throw new ArgumentNullException("datum");
            if (null == linearUnit) throw new ArgumentNullException("linearUnit");
            Contract.Requires(name != null);
            Datum = datum;
            Unit = linearUnit;
            Axes = Array.AsReadOnly(null == axes ? new IAxis[0] : axes.ToArray());
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Datum != null);
            Contract.Invariant(Unit != null);
            Contract.Invariant(Axes != null);
        }

        /// <inheritdoc/>
        public IDatumGeodetic Datum { get; private set; }

        /// <inheritdoc/>
        public IUnit Unit { get; private set; }

        /// <inheritdoc/>
        public IList<IAxis> Axes { get; private set; }

    }
}
