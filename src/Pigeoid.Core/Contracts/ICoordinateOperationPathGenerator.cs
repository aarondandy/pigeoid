namespace Pigeoid.Contracts
{
	public interface ICoordinateOperationPathGenerator<in TItem>
	{
		ICoordinateOperationCrsPathInfo Generate(TItem from, TItem to);
	}
}
