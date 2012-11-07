using System;
using JetBrains.Annotations;

namespace Pigeoid.Interop
{
	public class CoordinateOperationNameNormalizedComparer : NameNormalizedComparerBase
	{

		public static readonly CoordinateOperationNameNormalizedComparer Default = new CoordinateOperationNameNormalizedComparer();

		public CoordinateOperationNameNormalizedComparer() : this(null) { }

		public CoordinateOperationNameNormalizedComparer(StringComparer comparer) : base(comparer) { }

		[ContractAnnotation("=>notnull")]
		public override string Normalize(string text) {
			text = base.Normalize(text);

			if (CoordinateOperationStandardNames.IsNormalizedName(text))
				return text;

			var conicFlipText = text.Replace("CONFORMAL CONIC", "CONIC CONFORMAL");
			if (CoordinateOperationStandardNames.IsNormalizedName(conicFlipText))
				return conicFlipText;

			if(text.EndsWith("AREA")) {
				var areaConicText = text + " CONIC";
				if (CoordinateOperationStandardNames.IsNormalizedName(areaConicText))
					return areaConicText;
			}

			return text;
		}

	}
}
