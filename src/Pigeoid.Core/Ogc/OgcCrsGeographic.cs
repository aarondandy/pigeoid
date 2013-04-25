using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A geographic CRS.
    /// </summary>
    public class OgcCrsGeographic : OgcNamedAuthorityBoundEntity, ICrsGeographic
    {

        /// <summary>
        /// Constructs a new geographic CRS.
        /// </summary>
        /// <param name="name">The name of the CRS.</param>
        /// <param name="datum">The datum the CRS is based on.</param>
        /// <param name="angularUnit">The angular unit of measure for the CRS.</param>
        /// <param name="axes">The axes defining the space.</param>
        /// <param name="authority">The authority.</param>
        public OgcCrsGeographic(
            string name,
            IDatumGeodetic datum,
            IUnit angularUnit,
            IEnumerable<IAxis> axes,
            IAuthorityTag authority = null
        )
            : base(name, authority)
        {
            if (null == datum) throw new ArgumentNullException("datum");
            if (null == angularUnit) throw new ArgumentNullException("angularUnit");
            Contract.Requires(name != null);
            Datum = datum;
            Unit = angularUnit;
            Axes = Array.AsReadOnly(null == axes ? new IAxis[0] : axes.ToArray());
        }

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
