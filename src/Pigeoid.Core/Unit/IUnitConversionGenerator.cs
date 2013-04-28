namespace Pigeoid.Unit
{
    public interface IUnitConversionGenerator<TValue>
    {
        IUnitConversion<TValue> GenerateConversion(IUnit from, IUnit to);
    }
}
