using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.Unit
{
    public abstract class SimpleUnitConversionMapBase : IUnitConversionMap<double>
    {

        protected SimpleUnitConversionMapBase(IEqualityComparer<IUnit> unitEqualityComparer = null) {
            EqualityComparer = unitEqualityComparer ?? UnitEqualityComparer.Default;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(EqualityComparer != null);
        }

        public IEqualityComparer<IUnit> EqualityComparer { get; private set; }

        protected bool AreUnitsMatching(IUnit a, IUnit b) {
            return EqualityComparer.Equals(a, b);
        }

        public abstract IEnumerable<IUnit> AllUnits { get; }

        public abstract IEnumerable<IUnitConversion<double>> GetConversionsTo(IUnit to);

        public abstract IEnumerable<IUnitConversion<double>> GetConversionsFrom(IUnit from);

    }
}
