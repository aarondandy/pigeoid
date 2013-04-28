using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A fitted CRS.
    /// </summary>
    public class OgcCrsFitted : OgcNamedAuthorityBoundEntity, ICrsFitted
    {

        /// <summary>
        /// Constructs a new fitted CRS.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="toBaseOperation">The operation which converts to <paramref name="baseCrs"/>.</param>
        /// <param name="baseCrs">The base CRS.</param>
        /// <param name="authority">The authority code of the CRS.</param>
        public OgcCrsFitted(
            string name,
            ICoordinateOperationInfo toBaseOperation,
            ICrs baseCrs,
            IAuthorityTag authority = null
        )
            : base(name, authority)
        {
            if (null == toBaseOperation) throw new ArgumentNullException("toBaseOperation");
            if (null == baseCrs) throw new ArgumentNullException("baseCrs");
            Contract.Requires(name != null);
            ToBaseOperation = toBaseOperation;
            BaseCrs = baseCrs;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(ToBaseOperation != null);
            Contract.Invariant(BaseCrs != null);
        }

        /// <inheritdoc/>
        public ICrs BaseCrs { get; private set; }

        /// <inheritdoc/>
        public ICoordinateOperationInfo ToBaseOperation { get; private set; }

    }
}
