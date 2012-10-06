namespace Pigeoid.Contracts
{
	public interface ICoordinateOperationPathGenerator<in TItem>
	{
		ICoordinateOperationInfo Generate(TItem from, TItem to);
	}
}
