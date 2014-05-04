using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;

namespace Pigeoid.Unit
{

    public class BinaryUnitConversionMap : SimpleUnitConversionMapBase
    {

        private readonly IUnitConversion<double> _forwardOperation;

        public BinaryUnitConversionMap(IUnitConversion<double> forwardOperation, IEqualityComparer<IUnit> unitEqualityComparer = null)
            : base(unitEqualityComparer) {
            if (null == forwardOperation) throw new ArgumentNullException("forwardOperation");
            Contract.EndContractBlock();
            _forwardOperation = forwardOperation;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_forwardOperation != null);
        }

        private IUnit FromDefined {
            get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                return _forwardOperation.From;
            }
        }

        private IUnit ToDefined {
            get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                return _forwardOperation.To;
            }
        }

        public override IEnumerable<IUnit> AllUnits {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<IUnit>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IUnit>>(), x => x != null));
                if (EqualityComparer.Equals(FromDefined, ToDefined))
                    return ArrayUtil.CreateSingleElementArray(FromDefined);
                return new[] {FromDefined, ToDefined};
            }
        }

        public override IEnumerable<IUnitConversion<double>> GetConversionsTo(IUnit to) {
            Contract.Ensures(Contract.Result<IEnumerable<IUnitConversion<double>>>() != null);
            if (AreUnitsMatching(ToDefined, to))
                return new[] { _forwardOperation };
            if (_forwardOperation.HasInverse && AreUnitsMatching(FromDefined, to))
                return new[] { _forwardOperation.GetInverse() };
            return Enumerable.Empty<IUnitConversion<double>>();
        }

        public override IEnumerable<IUnitConversion<double>> GetConversionsFrom(IUnit from) {
            Contract.Ensures(Contract.Result<IEnumerable<IUnitConversion<double>>>() != null);
            if (AreUnitsMatching(FromDefined, from))
                return new[] { _forwardOperation };
            if (_forwardOperation.HasInverse && AreUnitsMatching(ToDefined, from))
                return new[] { _forwardOperation.GetInverse() };
            return Enumerable.Empty<IUnitConversion<double>>();
        }

    }

}
