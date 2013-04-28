using System;
using System.Diagnostics.Contracts;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// An OGC entity with a name and an authority tag.
    /// </summary>
    public abstract class OgcNamedAuthorityBoundEntity : INamedAuthorityBoundEntity
    {

        protected OgcNamedAuthorityBoundEntity(string name, IAuthorityTag authorityTag) {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();
            Name = name;
            Authority = authorityTag;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Name != null);
        }

        public IAuthorityTag Authority { get; private set; }

        public string Name { get; private set; }
    }
}
