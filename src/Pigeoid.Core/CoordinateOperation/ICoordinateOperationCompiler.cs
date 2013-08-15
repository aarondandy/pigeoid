using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation
{

    public interface ICoordinateOperationCompiler
    {
        ITransformation Compile(ICoordinateOperationCrsPathInfo operationPath);
    }

}
