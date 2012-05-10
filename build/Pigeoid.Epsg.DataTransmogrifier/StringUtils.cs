using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public static class StringUtils
	{

		public static string TrimTrailingPeriod(string text) {
			if(!String.IsNullOrEmpty(text) && text.Last() == '.') {
				return text.Substring(0, text.Length - 1);
			}
			return text;
		}

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
				return LetterClass.Space;
			}
			if (Char.IsWhiteSpace(c)) {
				return LetterClass.Space;
			}
			return LetterClass.Space; // spaces and other crap tend to be together so just count it as spaces to keep them together
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

		public static IEnumerable<int> GenerateWordIndices(List<string> wordLookup, string text) {
			foreach(var word in BreakIntoWordParts(text)) {
				var index = wordLookup.IndexOf(word);
				if(index < 0)
					throw new InvalidDataException();
				yield return index;
			}
		}


		internal static byte[] To7BitArray(IEnumerable<int> nums) {
			var data = new List<byte>();
			foreach (int n in nums) {
				// Write out an int 7 bits at a time. The high bit of the byte,
				// when on, tells reader to continue reading more bytes.
				var v = (uint)n; // support negative numbers
				while (v >= 0x80) {
					data.Add(unchecked((byte)(v | 0x80)));
					v >>= 7;
				}
				data.Add((byte)v);
			}
			return data.ToArray();
		}

		public static byte[] GenerateWordIndexBytes(List<string> wordLookup, string text) {
			return To7BitArray(GenerateWordIndices(wordLookup, text));
		} 

		public static int OverlapIndex(string a, string b) {
			for(int i = Math.Max(0,a.Length-b.Length); i < a.Length; i++) {
				if(a.Substring(i) == b.Substring(0,a.Length-i)) {
					return i;
				}
			}
			return -1;
		}

	}
}
