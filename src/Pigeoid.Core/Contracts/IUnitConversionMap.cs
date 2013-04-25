using System.Collections.Generic;

namespace Pigeoid.Contracts
{
    public interface IUnitConversionMap<TValue>
    {

        IEnumerable<IUnit> AllUnits { get; }

        IEnumerable<IUnitConversion<TValue>> GetConversionsTo(IUnit to);

        IEnumerable<IUnitConversion<TValue>> GetConversionsFrom(IUnit from);

        IEqualityComparer<IUnit> EqualityComparer { get; }

    }
}
