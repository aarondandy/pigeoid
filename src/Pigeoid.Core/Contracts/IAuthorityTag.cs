using System;
using System.Diagnostics.Contracts;

namespace Pigeoid.Contracts
{
    /// <summary>
    /// Definition of an authority tag complete with an authority name and item code.
    /// </summary>
    [ContractClass(typeof(IAuthorityTagCodeContracts))]
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

    [ContractClassFor(typeof(IAuthorityTag))]
    internal abstract class IAuthorityTagCodeContracts : IAuthorityTag
    {

        public string Name {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                throw new NotImplementedException();
            }
        }

        public string Code {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                throw new NotImplementedException();
            }
        }

        public abstract bool Equals(IAuthorityTag other);

    }
}
