using System;

namespace Pigeoid
{
    /// <summary>
    /// Definition of an authority tag complete with an authority name and item code.
    /// </summary>
    public interface IAuthorityTag : IEquatable<IAuthorityTag>
    {
        /// <summary>
        /// The name of the authority.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The authority tag code.
        /// </summary>
        string Code { get; }
    }

}
