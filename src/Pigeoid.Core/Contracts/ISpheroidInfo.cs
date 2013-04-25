using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{
    public interface ISpheroidInfo : ISpheroid<double>, IAuthorityBoundEntity
    {

        string Name { get; }

        IUnit AxisUnit { get; }

    }
}
