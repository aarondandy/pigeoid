using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{

    public interface ICoordinateOperationCompiler
    {
        ITransformation Compile(ICoordinateOperationCrsPathInfo operationPath);
    }

}
