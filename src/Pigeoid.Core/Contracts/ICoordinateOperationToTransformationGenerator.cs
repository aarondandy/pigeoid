using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{

	public interface ICoordinateOperationToTransformationGenerator
	{
		ITransformation Create(ICoordinateOperationCrsPathInfo operation);
	}

}
