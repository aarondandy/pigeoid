namespace Pigeoid.Contracts
{
    /// <summary>
    /// A named parameter. Used primarily for interoperability and serialization of transformations.
    /// </summary>
    public interface INamedParameter
    {
        string Name { get; }
        object Value { get; }
        IUnit Unit { get; }
    }

}
