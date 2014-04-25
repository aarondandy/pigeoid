using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Pigeoid.Unit
{
    public class ReadOnlyUnitConversionMap : SimpleUnitConversionMapBase
    {

        private readonly IUnitConversion<double>[] _allConversions;
        private readonly Dictionary<IUnit, IUnitConversion<double>[]> _fromMap;
        private readonly Dictionary<IUnit, IUnitConversion<double>[]> _toMap;
        private readonly IUnit[] _allDistinctUnits;

        public ReadOnlyUnitConversionMap(IEnumerable<IUnitConversion<double>> conversions, IEqualityComparer<IUnit> unitEqualityComparer = null)
            : base(unitEqualityComparer) {
            if (null == conversions) throw new ArgumentNullException("conversions");
            Contract.EndContractBlock();

            _allConversions = conversions.ToArray();
            _allDistinctUnits = _allConversions
                .Select(x => x.From)
                .Concat(_allConversions.Select(x => x.To))
                .Distinct(EqualityComparer)
                .ToArray();
            _fromMap = _allConversions
                .ToLookup(x => x.From, EqualityComparer)
                .ToDictionary(x => x.Key, x => x.ToArray(), EqualityComparer);
            _toMap = _allConversions
                .ToLookup(x => x.To, EqualityComparer)
                .ToDictionary(x => x.Key, x => x.ToArray(), EqualityComparer);
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_allConversions != null);
            Contract.Invariant(_allDistinctUnits != null);
            Contract.Invariant(_fromMap != null);
            Contract.Invariant(Contract.ForAll(_fromMap.Values, v => v != null));
            Contract.Invariant(_toMap != null);
            Contract.Invariant(Contract.ForAll(_toMap.Values, v => v != null));
        }

        public override IEnumerable<IUnit> AllUnits {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<IUnit>>() != null);
                return Array.AsReadOnly(_allDistinctUnits);
            }
        }

        public override IEnumerable<IUnitConversion<double>> GetConversionsTo(IUnit to) {
            Contract.Ensures(Contract.Result<IEnumerable<IUnitConversion<double>>>() != null);
            IUnitConversion<double>[] rawOperations;
            var results = new List<IUnitConversion<double>>();
            if (_toMap.TryGetValue(to, out rawOperations)) {
                Contract.Assume(rawOperations != null);
                results.AddRange(rawOperations);
            }
            if (_fromMap.TryGetValue(to, out rawOperations)) {
                Contract.Assume(rawOperations != null);
                results.AddRange(rawOperations.Where(x => x.HasInverse).Select(x => x.GetInverse()));
            }
            return results;
        }

        public override IEnumerable<IUnitConversion<double>> GetConversionsFrom(IUnit from) {
            Contract.Ensures(Contract.Result<IEnumerable<IUnitConversion<double>>>() != null);
            IUnitConversion<double>[] rawOperations;
            var results = new List<IUnitConversion<double>>();
            if (_fromMap.TryGetValue(from, out rawOperations)) {
                Contract.Assume(rawOperations != null);
                results.AddRange(rawOperations);
            }
            if (_toMap.TryGetValue(from, out rawOperations)) {
                Contract.Assume(rawOperations != null);
                results.AddRange(rawOperations.Where(x => x.HasInverse).Select(x => x.GetInverse()));
            }
            return results;
        }

    }
}
