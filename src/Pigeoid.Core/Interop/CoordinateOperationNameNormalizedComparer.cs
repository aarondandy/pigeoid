using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Pigeoid.Interop
{
	public class CoordinateOperationNameNormalizedComparer : NameNormalizedComparerBase
	{

		private static readonly Regex VariantEndingRegex = new Regex("VARIANT([A-Z])", RegexOptions.Compiled);

		public static readonly CoordinateOperationNameNormalizedComparer Default = new CoordinateOperationNameNormalizedComparer();

		public CoordinateOperationNameNormalizedComparer() : this(null) { }

		public CoordinateOperationNameNormalizedComparer(StringComparer comparer) : base(comparer) { }

		[ContractAnnotation("=>notnull")]
		public override string Normalize(string text) {
			if (null == text)
				return String.Empty;

			text = base.Normalize(text);

			// take the 'orientated' off the end
			if (text.EndsWith("NORTHORIENTATED"))
				text = text.Substring(0, text.Length - 10);

			text = text.Replace("LONGITUDEROTATION", "GEOGRAPHICOFFSET");
			if (text.EndsWith("OFFSETS"))
				text = text.Substring(0, text.Length - 1);

			if (CoordinateOperationStandardNames.IsNormalizedName(text))
				return text;

			var orientated = text.Replace("ORIENTED", "ORIENTATED");
			if (CoordinateOperationStandardNames.IsNormalizedName(orientated))
				return orientated;

			var variantMatch = VariantEndingRegex.Match(text);
			if (variantMatch.Success){
				var variantLetter = variantMatch.Groups[1].Value;
				var nonVariant = text.Substring(0, variantMatch.Index);

				if (variantLetter == "A"){
					var sp1Replace = String.Concat(nonVariant, "1SP");
					if (CoordinateOperationStandardNames.IsNormalizedName(sp1Replace))
						return sp1Replace;
				}

				if (variantLetter == "B"){
					var sp2Replace = String.Concat(nonVariant, "2SP");
					if (CoordinateOperationStandardNames.IsNormalizedName(sp2Replace))
						return sp2Replace;
				}

				if (CoordinateOperationStandardNames.IsNormalizedName(nonVariant))
					return nonVariant;
			}

			if (text.EndsWith("PROJECTION")) {
				var removedProjectionText = text.Substring(0, text.Length - 10);
				if (CoordinateOperationStandardNames.IsNormalizedName(removedProjectionText))
					return removedProjectionText;
			}

			if (text.EndsWith("SPHERICAL")) {
				if (text.StartsWith("MERCATOR")) {
					var nonSpherical = text.Substring(0, text.Length - 9);
					if (CoordinateOperationStandardNames.IsNormalizedName(nonSpherical))
						return nonSpherical;
				}
			}

			var conicFlipText = text.Replace("CONFORMALCONIC", "CONICCONFORMAL");
			if (CoordinateOperationStandardNames.IsNormalizedName(conicFlipText))
				return conicFlipText;

			if(text.EndsWith("AREA")) {
				var areaConicText = text + "CONIC";
				if (CoordinateOperationStandardNames.IsNormalizedName(areaConicText))
					return areaConicText;
			}

			return text;
		}

	}
}
