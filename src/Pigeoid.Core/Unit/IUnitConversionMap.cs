using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.Unit
{
    [ContractClass(typeof(IUnitConversionMapCodeContracts<>))]
    public interface IUnitConversionMap<TValue>
    {

        IEnumerable<IUnit> AllUnits { get; }

        IEnumerable<IUnitConversion<TValue>> GetConversionsTo(IUnit to);

        IEnumerable<IUnitConversion<TValue>> GetConversionsFrom(IUnit from);

        IEqualityComparer<IUnit> EqualityComparer { get; }

    }

    [ContractClassFor(typeof(IUnitConversionMap<>))]
    internal abstract class IUnitConversionMapCodeContracts<TValue> : IUnitConversionMap<TValue>
    {

        public IEnumerable<IUnit> AllUnits {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<IUnit>>() != null);
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IUnitConversion<TValue>> GetConversionsTo(IUnit to) {
            Contract.Requires(to != null);
            throw new NotImplementedException();
        }

        public IEnumerable<IUnitConversion<TValue>> GetConversionsFrom(IUnit from) {
            Contract.Requires(from != null);
            throw new NotImplementedException();
        }

        public IEqualityComparer<IUnit> EqualityComparer {
            get {
                Contract.Ensures(Contract.Result<IEqualityComparer<IUnit>>() != null);
                throw new NotImplementedException();
            }
        }
    }
}
