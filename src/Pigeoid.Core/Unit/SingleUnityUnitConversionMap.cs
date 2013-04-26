using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Unit
{
    [Obsolete("I don't see a use for this. Yagni?")]
    public class SingleUnityUnitConversionMap : SimpleUnitConversionMapBase
    {

        private readonly IUnit _singleUnit;

        public SingleUnityUnitConversionMap(IUnit unit, IEqualityComparer<IUnit> unitEqualityComparer = null)
            : base(unitEqualityComparer) {
            if (null == unit) throw new ArgumentNullException("unit");
            Contract.EndContractBlock();
            _singleUnit = unit;
        }

        private void CodeContractInvariants() {
            Contract.Invariant(_singleUnit != null);
        }

        public override IEnumerable<IUnit> AllUnits {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<IUnit>>() != null);
                return new[] { _singleUnit };
            }
        }

        public override IEnumerable<IUnitConversion<double>> GetConversionsTo(IUnit to) {
            Contract.Ensures(Contract.Result<IEnumerable<IUnitConversion<double>>>() != null);
            return AreUnitsMatching(_singleUnit, to)
                ? new[] { new UnitUnityConversion(_singleUnit, to) }
                : Enumerable.Empty<IUnitConversion<double>>();
        }

        public override IEnumerable<IUnitConversion<double>> GetConversionsFrom(IUnit from) {
            Contract.Ensures(Contract.Result<IEnumerable<IUnitConversion<double>>>() != null);
            return AreUnitsMatching(_singleUnit, from)
                ? new[] { new UnitUnityConversion(from, _singleUnit) }
                : Enumerable.Empty<IUnitConversion<double>>();
        }
    }
}
