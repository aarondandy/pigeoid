using System.Diagnostics.Contracts;

namespace Pigeoid.Contracts
{
    [ContractClass(typeof(IDatumCodeContract))]
    public interface IDatum : INamedAuthorityBoundEntity
    {
        string Type { get; }
    }

    [ContractClassFor(typeof(IDatum))]
    internal abstract class IDatumCodeContract : IDatum
    {

        public string Type {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new System.NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract IAuthorityTag Authority { get; }
    }

}
