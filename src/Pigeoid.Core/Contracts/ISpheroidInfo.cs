using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{
    public interface ISpheroidInfo : ISpheroid<double>, INamedAuthorityBoundEntity
    {
        IUnit AxisUnit { get; }
    }
}
