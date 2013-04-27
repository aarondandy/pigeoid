using System.Diagnostics.Contracts;

namespace Pigeoid.Contracts
{
    /// <summary>
    /// A named parameter. Used primarily for interoperability and serialization of transformations.
    /// </summary>
    [ContractClass(typeof(INamedParameterCodeContract))]
    public interface INamedParameter
    {
        string Name { get; }
        object Value { get; }
        IUnit Unit { get; }
    }

    [ContractClassFor(typeof(INamedParameter))]
    internal abstract class INamedParameterCodeContract : INamedParameter
    {

        public string Name {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new System.NotImplementedException();
            }
        }

        public abstract object Value { get; }

        public abstract IUnit Unit { get; }
    }

}
