using System;
using JetBrains.Annotations;

namespace Pigeoid.Interop
{
	public class ParameterNameComparer : NameNormalizedComparerBase
	{

		public static readonly ParameterNameComparer Default = new ParameterNameComparer();

		public ParameterNameComparer() : this(null) { }

		public ParameterNameComparer(StringComparer comparer) : base(comparer) { }

		[ContractAnnotation("=>notnull")]
		public override string Normalize(string text) {
			text = base.Normalize(text);
			text = text.Replace("LATITUDE", "LAT");
			text = text.Replace("LONGITUDE", "LON");
			return text;
		}

	}
}
