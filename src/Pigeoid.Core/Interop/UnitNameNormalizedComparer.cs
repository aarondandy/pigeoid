using System;
using System.Diagnostics.Contracts;

namespace Pigeoid.Interop
{
    public class UnitNameNormalizedComparer : NameNormalizedComparerBase
    {

        private static readonly string[] PluralSuffixInTextWords = new[] {
            "METER",
            "FATHOM",
            "MILE",
            "CHAIN",
            "LINK",
            "YARD",
            "RADIAN",
            "DEGREE",
            "MINUTE",
            "SECOND",
            "HOUR",
            "GRAD",
            "PART"
        };

        private static readonly string[] PluralSuffixWholeWords = new[] {
            "COEFFICIENT",
            "GON"
        };

        private static string RemovePluralS(string text, string wordBase) {
            Contract.Requires(text != null);
            Contract.Requires(wordBase != null);
            int partIndex = text.LastIndexOf(wordBase, StringComparison.OrdinalIgnoreCase);
            while (partIndex >= 0 && partIndex < (text.Length - wordBase.Length)) {
                var sIndex = partIndex + wordBase.Length;
                if (text[sIndex] == 'S')
                    text = text.Remove(sIndex, 1);

                partIndex = partIndex > 0
                    ? text.LastIndexOf(wordBase, partIndex - 1, StringComparison.OrdinalIgnoreCase)
                    : -1;
            }
            return text;
        }

        public static readonly UnitNameNormalizedComparer Default = new UnitNameNormalizedComparer();

        public override string Normalize(string text) {
            if (null == text)
                return String.Empty;

            text = base.Normalize(text);

            // to avoid confusion, just treat coefficient as unity, just another number
            if (text.Equals("COEFFICIENT"))
                return "UNITY";

            // uhh-mur-eh-cun-eyes the names
            text = text.Replace("METRE", "METER");

            // convert plural words to singular
            text = text.Replace("FEET", "FOOT");
            text = text.Replace("UNITIES", "UNITY");

            // remove plural suffix
            for (int i = 0; i < PluralSuffixWholeWords.Length; i++) {
                var baseWord = PluralSuffixWholeWords[i];
                Contract.Assume(baseWord != null);
                if (text.Length == baseWord.Length - 1 && text[text.Length - 1] == 'S' && text.StartsWith(baseWord, StringComparison.OrdinalIgnoreCase))
                    return baseWord;
            }

            for (int i = 0; i < PluralSuffixInTextWords.Length; i++) {
                var baseWord = PluralSuffixInTextWords[i];
                Contract.Assume(baseWord != null);
                text = RemovePluralS(text, baseWord);
            }

            return text;
        }

    }
}
