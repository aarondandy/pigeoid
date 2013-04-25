using System;
using System.Diagnostics.Contracts;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
    public class OgcCoordinateOperationMethodInfo : ICoordinateOperationMethodInfo
    {

        public OgcCoordinateOperationMethodInfo(string name, IAuthorityTag authorityTag = null) {
            if(name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();
            Name = name;
            Authority = authorityTag;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Name != null);
        }

        public string Name { get; private set; }

        public IAuthorityTag Authority { get; private set; }
    }
}
