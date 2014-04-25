using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.CoordinateOperation;
using Pigeoid.Unit;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A projected CRS.
    /// </summary>
    public class OgcCrsProjected : OgcNamedAuthorityBoundEntity, ICrsProjected
    {

        /// <summary>
        /// Constructs a new projected CRS.
        /// </summary>
        /// <param name="name">The name of the CRS.</param>
        /// <param name="baseCrs">The CRS this CRS is based on.</param>
        /// <param name="projection">The projection operation.</param>
        /// <param name="linearUnit">The linear unit of the projection.</param>
        /// <param name="axes">The axes of the projected CRS.</param>
        /// <param name="authority">The authority.</param>
        public OgcCrsProjected(
            string name,
            ICrsGeodetic baseCrs,
            ICoordinateOperationInfo projection,
            IUnit linearUnit,
            IEnumerable<IAxis> axes,
            IAuthorityTag authority = null
        )
            : base(name, authority) {
            if (null == baseCrs) throw new ArgumentNullException("baseCrs");
            if (null == projection) throw new ArgumentNullException("projection");
            if (null == linearUnit) throw new ArgumentNullException("linearUnit");
            if(null == axes) throw new ArgumentNullException("axes");
            Contract.Requires(name != null);

            BaseCrs = baseCrs;
            Projection = projection;
            Unit = linearUnit;
            Axes = Array.AsReadOnly(null == axes ? new IAxis[0] : axes.ToArray());
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(BaseCrs != null);
            Contract.Invariant(Unit != null);
            Contract.Invariant(Projection != null);
            Contract.Invariant(Axes != null);
        }

        /// <inheritdoc/>
        public ICrsGeodetic BaseCrs { get; private set; }

        /// <inheritdoc/>
        public IUnit Unit { get; private set; }

        /// <inheritdoc/>
        public ICoordinateOperationInfo Projection { get; private set; }

        /// <inheritdoc/>
        public IList<IAxis> Axes { get; private set; }

        /// <inheritdoc/>
        public IDatumGeodetic Datum { get { return BaseCrs.Datum; } }

    }
}
