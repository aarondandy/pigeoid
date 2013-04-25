namespace Pigeoid.Contracts
{
    public interface IPassThroughCoordinateOperationInfo : IConcatenatedCoordinateOperationInfo
    {

        int FirstAffectedOrdinate { get; }

    }
}
