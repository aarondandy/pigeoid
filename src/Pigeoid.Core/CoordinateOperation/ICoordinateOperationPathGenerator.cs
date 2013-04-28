namespace Pigeoid.CoordinateOperation
{
    public interface ICoordinateOperationPathGenerator<in TItem>
    {
        ICoordinateOperationCrsPathInfo Generate(TItem from, TItem to);
    }
}
