using System;
using System.Diagnostics.Contracts;

namespace Pigeoid
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

    [ContractClass(typeof(INamedAuthorityBoundEntityCodeContracts))]
    public interface INamedAuthorityBoundEntity : IAuthorityBoundEntity
    {
        /// <summary>
        /// The friendly name given to the entity.
        /// </summary>
        string Name { get; }
    }

    [ContractClassFor(typeof(INamedAuthorityBoundEntity))]
    internal abstract class INamedAuthorityBoundEntityCodeContracts : INamedAuthorityBoundEntity
    {

        public string Name {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                throw new NotImplementedException();
            }
        }

        public abstract IAuthorityTag Authority { get; }
    }
}
