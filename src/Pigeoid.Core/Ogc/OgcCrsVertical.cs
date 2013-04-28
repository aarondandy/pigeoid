using System;
using System.Diagnostics.Contracts;
using Pigeoid.Unit;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A vertical CRS.
    /// </summary>
    public class OgcCrsVertical : OgcNamedAuthorityBoundEntity, ICrsVertical
    {

        /// <summary>
        /// Constructs a new vertical CRS.
        /// </summary>
        /// <param name="name">The name of the CRS.</param>
        /// <param name="datum">The datum the CRS is based on.</param>
        /// <param name="linearUnit">The linear unit for the CRS.</param>
        /// <param name="axis">The axis for the linear CRS.</param>
        /// <param name="authority">The authority.</param>
        public OgcCrsVertical(
            string name,
            IDatum datum,
            IUnit linearUnit,
            IAxis axis,
            IAuthorityTag authority
        )
            : base(name, authority)
        {
            if(datum == null) throw new ArgumentNullException("datum");
            if(linearUnit == null) throw new ArgumentNullException("linearUnit");
            if(axis == null) throw new ArgumentNullException("axis");
            Contract.Requires(name != null);
            Datum = datum;
            Unit = linearUnit;
            Axis = axis;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Datum != null);
            Contract.Invariant(Unit != null);
            Contract.Invariant(Axis != null);
        }

        public IDatum Datum { get; private set; }

        public IUnit Unit { get; private set; }

        public IAxis Axis { get; private set; }
    }
}
