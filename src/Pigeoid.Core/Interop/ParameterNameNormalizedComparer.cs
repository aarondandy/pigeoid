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
			text = base.Normalize(text);
			text = text.Replace("LATITUDE", "LAT");
			text = text.Replace("LONGITUDE", "LON");
			return text;
		}

	}
}
