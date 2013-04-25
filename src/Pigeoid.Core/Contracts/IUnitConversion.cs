using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{
    public interface IUnitConversion<TValue> : ITransformation<TValue>
    {
        IUnit From { get; }
        IUnit To { get; }
        new IUnitConversion<TValue> GetInverse();
    }

    public interface IUnitScalarConversion<TValue> : IUnitConversion<TValue>
    {
        TValue Factor { get; }
        new IUnitScalarConversion<TValue> GetInverse();
    }

}
