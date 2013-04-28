namespace Pigeoid.CoordinateOperation
{
    public interface IPassThroughCoordinateOperationInfo : IConcatenatedCoordinateOperationInfo
    {

        int FirstAffectedOrdinate { get; }

    }
}
