using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public static class StringUtils
	{

		static bool SplitBetween(char a, char b) {
			return SplitBetween(Classify(a), Classify(b));
		}

		static bool SplitBetween(LetterClass a, LetterClass b) {
			return a != b
				|| a == LetterClass.Other
				|| b == LetterClass.Other
			;
		}

		public static string[] BreakIntoWordParts(string text) {
			if (null == text)
				return new string[0];
			var breaks = new List<int>();
			for (int i = 1; i < text.Length; i++) {
				bool split = SplitBetween(text[i - 1], text[i]);
				if (split) {
					breaks.Add(i);
				}
			}
			string[] words = new string[breaks.Count + 1];
			if (0 == breaks.Count) {
				words[0] = text;
			}
			else {
				words[words.Length - 1] = text.Substring(breaks[breaks.Count - 1]);
				words[0] = text.Substring(0, breaks[0]);
				for (int i = 1; i < breaks.Count; i++) {
					words[i] = text.Substring(breaks[i - 1], breaks[i] - breaks[i - 1]);
				}
			}
			return words;
		}

		enum LetterClass
		{
			// TODO: see if the file size/extraction performance is better if we combine Text and Number together to cause fewer word breaks
			Text,
			Number,
			Other,
			Space
		}

		static LetterClass Classify(char c) {
			if (Char.IsLetter(c)) {
				return LetterClass.Text;
			}
			if (Char.IsDigit(c)) {
				return LetterClass.Number;
			}
			if (Char.IsWhiteSpace(c)) {
				return LetterClass.Space;
			}
			return LetterClass.Other;
		}

		public static Dictionary<string, int> BuildWordCountLookup(IEnumerable<string> wordList) {
			var wordCounts = new Dictionary<string, int>();
			foreach (var word in wordList) {
				int currentCount;
				if (wordCounts.TryGetValue(word, out currentCount)) {
					wordCounts[word] = currentCount + 1;
				}
				else {
					wordCounts[word] = 1;
				}
			}
			return wordCounts;
		}

	}
}
