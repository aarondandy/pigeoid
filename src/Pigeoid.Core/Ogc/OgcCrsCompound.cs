using System;
using System.Diagnostics.Contracts;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A compound CRS composed of a head and tail CRS.
    /// </summary>
    public sealed class OgcCrsCompound : OgcNamedAuthorityBoundEntity, ICrsCompound
    {

        /// <summary>
        /// Creates a new compound CRS.
        /// </summary>
        /// <param name="name">The name of the compound.</param>
        /// <param name="head">The head CRS.</param>
        /// <param name="tail">The tail CRS.</param>
        /// <param name="authority">The authority.</param>
        public OgcCrsCompound(
            string name,
            ICrs head,
            ICrs tail,
            IAuthorityTag authority
        )
            : base(name, authority)
        {
            if(null == head) throw new ArgumentNullException("head");
            if(null == tail) throw new ArgumentNullException("tail");
            Contract.Requires(name != null);
            Head = head;
            Tail = tail;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Head != null);
            Contract.Invariant(Tail != null);
        }

        /// <summary>
        /// The head CRS.
        /// </summary>
        public ICrs Head { get; private set; }

        /// <summary>
        /// The tail CRS.
        /// </summary>
        public ICrs Tail { get; private set; }

    }
}
