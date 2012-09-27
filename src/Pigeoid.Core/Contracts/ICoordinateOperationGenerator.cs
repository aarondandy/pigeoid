namespace Pigeoid.Contracts
{
	public interface ICoordinateOperationGenerator<in TItem>
	{
		ICoordinateOperationInfo Generate(TItem from, TItem to);
	}
}
