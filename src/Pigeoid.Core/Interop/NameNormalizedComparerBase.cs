using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Pigeoid.Interop
{
	public abstract class NameNormalizedComparerBase : IComparer<string>, IEqualityComparer<string>
	{

		protected NameNormalizedComparerBase() : this(null) { }

		protected NameNormalizedComparerBase(StringComparer coreTextCompare) {
			CoreTextCompare = coreTextCompare ?? StringComparer.OrdinalIgnoreCase;
		}

		public StringComparer CoreTextCompare { get; private set; }

		[ContractAnnotation("=>notnull")]
		public static string NormalizeBasic(string text) {
			if (null == text)
				return String.Empty;

			var builder = new StringBuilder();
			for(int i = 0; i < text.Length; i++) {
				var c = text[i];
				if(Char.IsLetterOrDigit(c)) {
					builder.Append(Char.ToUpperInvariant(c));
				}
			}
			return builder.ToString();
		}

		[ContractAnnotation("=>notnull")]
		public virtual string Normalize(string text) {
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
