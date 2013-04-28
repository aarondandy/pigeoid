using Vertesaur.Contracts;

namespace Pigeoid.CoordinateOperation
{

    public interface ICoordinateOperationCompiler
    {
        ITransformation Compile(ICoordinateOperationCrsPathInfo operationPath);
    }

}
