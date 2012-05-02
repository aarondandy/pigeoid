using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Dapper;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class Program
	{
		static string GetDatabasePath(string dataFolderPath) {
			return new DirectoryInfo(dataFolderPath)
				.GetFiles("*.mdb", SearchOption.AllDirectories)
				.Select(fi => fi.FullName)
				.FirstOrDefault();
		}

		static void Main(string[] args) {

			string dataFolderPath = "data";
			if (args.Length > 0 && !String.IsNullOrWhiteSpace(args[0]))
				dataFolderPath = args[0];

			if (!Directory.Exists(dataFolderPath))
				throw new IOException("Folder does not exist: " + dataFolderPath);

			var dataFilePath = GetDatabasePath(dataFolderPath);
			if(null == dataFilePath || !File.Exists(dataFilePath))
				throw new FileNotFoundException("Database file not found.");
			Console.WriteLine("Found database: " + dataFilePath);

			var connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dataFilePath;
			using(var connection = new OleDbConnection(connectionString)) {
				connection.Open();
				Transmogrify(connection,Path.Combine(dataFolderPath,"out"));
			}

		}

		private static void Transmogrify(IDbConnection connection, string outFolder) {

			// generate a list of words
			var wordLookup = GenerateWordLookup(connection, new Dictionary<string, Func<dynamic,dynamic>>{
				{"Alias",o => o.ALIAS},
				{"Area",o => o.AREA_NAME},
				{"Coordinate Axis", o => o.COORD_AXIS_ORIENTATION},
				{"Coordinate Axis Name", o => o.COORD_AXIS_NAME},
				{"Coordinate Reference System",o => o.COORD_REF_SYS_NAME},
				{"Coordinate System",o => o.COORD_SYS_NAME},
				{"Coordinate_Operation",o => o.COORD_OP_NAME},
				{"Coordinate_Operation Method",o => o.COORD_OP_METHOD_NAME},
				{"Coordinate_Operation Parameter",o => o.PARAMETER_NAME},
				{"Coordinate_Operation Parameter Value",o => o.PARAM_VALUE_FILE_REF},
				{"Datum",o => o.DATUM_NAME},
				{"Ellipsoid",o => o.ELLIPSOID_NAME},
				{"Prime Meridian",o => o.PRIME_MERIDIAN_NAME},
				{"Unit of Measure",o => o.UNIT_OF_MEAS_NAME},
			});
			var words = wordLookup
				.OrderByDescending(e => e.Value)
				.Select(e => e.Key)
				.ToList();

			if (!Directory.Exists(outFolder))
				Directory.CreateDirectory(outFolder);

			// save the list of words to the DAT file)
			using (var stream = File.Open(Path.Combine(outFolder,"words.dat"), FileMode.Create))
				WordsToDatFile(words, stream);
		}

		private static void WordsToDatFile(List<string> words, FileStream stream) {
			using (var writer = new BinaryWriter(stream)) {
				writer.Write((int)words.Count);
				foreach (var word in words) {
					var textBytes = Encoding.UTF8.GetBytes(word);
					writer.Write((byte)textBytes.Length);
					writer.Write(textBytes);
				}
			}
		}

		private static Dictionary<string, int> GenerateWordLookup(IDbConnection connection, Dictionary<string, Func<dynamic, dynamic>> wordSources) {
			var wordCounts = new Dictionary<string, int>();
			foreach(var table in wordSources) {
				var queryString = String.Format("Select * from [{0}]",table.Key);
				foreach (var result in connection.Query(queryString)) {
					var data = table.Value(result) as string;
					foreach(var word in BreakIntoWordParts(data)) {
						int wordCount;
						wordCounts[word] = wordCounts.TryGetValue(word, out wordCount)
							? wordCount + 1
							: 1;
					}
				}
				;
			}
			return wordCounts;
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

		static string[] BreakIntoWordParts(string text) {
			if(null == text)
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
			if (' ' == c) {
				return LetterClass.Space;
			}
			return LetterClass.Other;
		}

	}
}
