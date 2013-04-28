using Pigeoid.Unit;
using Vertesaur.Contracts;

namespace Pigeoid
{
    public interface ISpheroidInfo : ISpheroid<double>, INamedAuthorityBoundEntity
    {
        IUnit AxisUnit { get; }
    }
}
