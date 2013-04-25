namespace Pigeoid.Contracts
{
    /// <summary>
    /// Provides access to the authority information that identifies some object.
    /// </summary>
    public interface IAuthorityBoundEntity
    {

        /// <summary>
        /// Accesses the authority tag.
        /// </summary>
        IAuthorityTag Authority { get; }

    }
}
