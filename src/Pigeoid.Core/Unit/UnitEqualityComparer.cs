using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.Interop;

namespace Pigeoid.Unit
{
    public class UnitEqualityComparer : IEqualityComparer<IUnit>
    {

        public static readonly UnitEqualityComparer Default = new UnitEqualityComparer();

        public UnitEqualityComparer(INameNormalizedComparer nameNormalizedComparer = null) {
            NameNormalizedComparer = nameNormalizedComparer ?? UnitNameNormalizedComparer.Default;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(NameNormalizedComparer != null);
        }

        public INameNormalizedComparer NameNormalizedComparer { get; private set; }

        public bool Equals(IUnit x, IUnit y) {
            if (ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;

            return AreSameType(x, y)
                && NameNormalizedComparer.Equals(x.Name, y.Name);
        }

        public int GetHashCode(IUnit obj) {
            if (null == obj)
                return 0;
            return NameNormalizedComparer.GetHashCode(obj.Name)
                ^ -NameNormalizedComparer.GetHashCode(obj.Type);
        }

        public bool AreSameType(IUnit x, IUnit y) {
            if (ReferenceEquals(x, y))
                return true;
            if (null == x || null == y)
                return false;
            return NameNormalizedComparer.Equals(x.Type, y.Type);
        }


    }
}
