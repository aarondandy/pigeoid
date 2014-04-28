using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.Interop
{

    [ContractClass(typeof(NameNormalizedComparerContracts))]
    public interface INameNormalizedComparer : IComparer<string>, IEqualityComparer<string>
    {
        string Normalize(string text);
    }

    [ContractClassFor(typeof(INameNormalizedComparer))]
    internal abstract class NameNormalizedComparerContracts : INameNormalizedComparer
    {

        public string Normalize(string text) {
            Contract.Ensures(Contract.Result<string>() != null);
            throw new NotImplementedException();
        }

        public abstract int Compare(string x, string y);

        public abstract bool Equals(string x, string y);

        public abstract int GetHashCode(string obj);
    }

}
