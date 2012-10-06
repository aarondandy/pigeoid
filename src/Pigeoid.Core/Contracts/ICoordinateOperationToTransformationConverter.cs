using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{
	public interface ICoordinateOperationToTransformationConverter
	{

		ITransformation Convert(ICoordinateOperationInfo operation);

	}
}
