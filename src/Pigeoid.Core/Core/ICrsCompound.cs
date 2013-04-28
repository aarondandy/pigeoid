using System;
using System.Diagnostics.Contracts;

namespace Pigeoid
{
    /// <summary>
    /// A compound coordinate reference system composed of multiple other coordinate reference systems.
    /// </summary>
    [ContractClass(typeof(ICrsCompountCodeContracts))]
    public interface ICrsCompound : ICrs
    {
        ICrs Head { get; }
        ICrs Tail { get; }
    }

    [ContractClassFor(typeof(ICrsCompound))]
    internal abstract class ICrsCompountCodeContracts : ICrsCompound
    {

        public ICrs Head {
            get {
                Contract.Ensures(Contract.Result<ICrs>() != null);
                throw new NotImplementedException();
            }
        }

        public ICrs Tail {
            get {
                Contract.Ensures(Contract.Result<ICrs>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract IAuthorityTag Authority { get; }
    }

}
