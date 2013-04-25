namespace Pigeoid.Contracts
{
    public interface ICoordinateOperationInfo
    {

        /// <summary>
        /// The name of the operation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines if this operation has an inverse.
        /// </summary>
        bool HasInverse { get; }

        /// <summary>
        /// Gets the inverse of this operation information if one exists.
        /// </summary>
        /// <returns>An operation.</returns>
        ICoordinateOperationInfo GetInverse();

        /// <summary>
        /// Flag is set when this represents the inverse of a well defined coordinate operation.
        /// </summary>
        bool IsInverseOfDefinition { get; }

    }
}
