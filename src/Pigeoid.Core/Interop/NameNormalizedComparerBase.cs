using System;
using System.Diagnostics.Contracts;
using System.Text;
using Pigeoid.Contracts;

namespace Pigeoid.Interop
{
    public abstract class NameNormalizedComparerBase : INameNormalizedComparer
    {

        protected NameNormalizedComparerBase() : this(null) { }

        protected NameNormalizedComparerBase(StringComparer coreTextCompare) {
            CoreTextCompare = coreTextCompare ?? StringComparer.OrdinalIgnoreCase;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(CoreTextCompare != null);
        }

        public StringComparer CoreTextCompare { get; private set; }

        public static string NormalizeBasic(string text) {
            Contract.Ensures(Contract.Result<string>() != null);
            if (null == text)
                return String.Empty;

            var builder = new StringBuilder();
            for (int i = 0; i < text.Length; i++) {
                var c = text[i];
                if (Char.IsLetterOrDigit(c)) {
                    builder.Append(Char.ToUpperInvariant(c));
                }
            }
            var result = builder.ToString();
            result = result.Replace("VISUALISATION", "VISUALIZATION");
            return result;
        }

        public virtual string Normalize(string text) {
            Contract.Ensures(Contract.Result<string>() != null);
            return NormalizeBasic(text);
        }

        public virtual int Compare(string x, string y) {
            return CoreTextCompare.Compare(Normalize(x), Normalize(y));
        }

        public virtual bool Equals(string x, string y) {
            return CoreTextCompare.Equals(Normalize(x), Normalize(y));
        }

        public virtual int GetHashCode(string text) {
            return CoreTextCompare.GetHashCode(Normalize(text));
        }

    }
}
