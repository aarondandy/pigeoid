using System;
using JetBrains.Annotations;

namespace Pigeoid.Interop
{
	public class ParameterNameNormalizedComparer : NameNormalizedComparerBase
	{

		public static readonly ParameterNameNormalizedComparer Default = new ParameterNameNormalizedComparer();

		public ParameterNameNormalizedComparer() : this(null) { }

		public ParameterNameNormalizedComparer(StringComparer comparer) : base(comparer) { }

		[ContractAnnotation("=>notnull")]
		public override string Normalize(string text) {
			if (null == text)
				return String.Empty;
			text = base.Normalize(text);
			text = text.Replace("LATITUDE", "LAT");
			text = text.Replace("LONGITUDE", "LON");
			text = text.Replace("CENTRE", "CENTER");
			return text;
		}

	}
}
