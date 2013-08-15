using Pigeoid.Unit;
using Vertesaur;

namespace Pigeoid
{
    public interface ISpheroidInfo : ISpheroid<double>, INamedAuthorityBoundEntity
    {
        IUnit AxisUnit { get; }
    }
}
