using System;
using System.IO;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

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

			var sessionFactory = Fluently.Configure()
				.Database(JetDriverConfiguration.Standard.ConnectionString(c =>
					c.DatabaseFile(dataFilePath)
					.Provider("Microsoft.Jet.OLEDB.4.0")
				))
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<Program>())
				.BuildSessionFactory();

			using(var session = sessionFactory.OpenSession()) {
				var epsgData = new EpsgData(session);
				var aliases = epsgData.Aliases;
				var areas = epsgData.Areas;
				var axes = epsgData.Axes;
				var axisNames = epsgData.AxisNames;
				var coordSys = epsgData.CoordinateSystems;
				var coordOps = epsgData.CoordinateOperations;
				var opMethods = epsgData.CoordinateOperationMethods;
				var crs = epsgData.Crs;
				var datums = epsgData.Datums;
				var deps = epsgData.Deprecations;
				var elps = epsgData.Ellipsoids;
				var nameSystems = epsgData.NamingSystems;
				var primeMeridians = epsgData.PrimeMeridians;
				var supersessions = epsgData.Supersessions;
				var uoms = epsgData.Uoms;
				var prms = epsgData.Parameters;
				var paramUses = epsgData.ParamUses;
				var paramValues = epsgData.ParamValues;
				var coordOpPathItems = epsgData.CoordOpPathItems;
				;
			}

		}
		/*
		private static void Transmogrify(ISession session, string outFolder) {

			var epsgData = new EpsgData();

			if (!Directory.Exists(outFolder))
				Directory.CreateDirectory(outFolder);

			// generate a list of words from used columns
			// TODO: make sure all of these are used, if not they need to be removed to save space!
			epsgData.AllWords = GenerateWordLookup(session, new Dictionary<string, Func<dynamic, dynamic>>{
				{"Alias", o => o.ALIAS},
				{"Area", o => o.AREA_NAME},
				{"Coordinate Axis", o => o.COORD_AXIS_ORIENTATION},
				{"Coordinate Axis Name", o => o.COORD_AXIS_NAME},
				{"Coordinate Reference System", o => o.COORD_REF_SYS_NAME},
				{"Coordinate System", o => o.COORD_SYS_NAME},
				{"Coordinate_Operation", o => o.COORD_OP_NAME},
				{"Coordinate_Operation Method", o => o.COORD_OP_METHOD_NAME},
				{"Coordinate_Operation Parameter", o => o.PARAMETER_NAME},
				{"Coordinate_Operation Parameter Value", o => o.PARAM_VALUE_FILE_REF},
				{"Datum", o => o.DATUM_NAME},
				{"Ellipsoid", o => o.ELLIPSOID_NAME},
				{"Prime Meridian", o => o.PRIME_MERIDIAN_NAME},
				{"Unit of Measure", o => o.UNIT_OF_MEAS_NAME},
			});

			// save the list of words to the DAT file
			using (var stream = File.Open(Path.Combine(outFolder, "words.dat"), FileMode.Create))
				WordsToDatFile(epsgData.AllWords, stream);

			// generate a list of numbers from used columns
			// TODO: make sure all of these are used, if not they need to be removed to save space!
			epsgData.AllNumbers = GenerateNumberValueLookup(connection, new Dictionary<string, List<Func<dynamic, dynamic>>>{
				{"Area", new List<Func<dynamic, dynamic>>{o => o.AREA_SOUTH_BOUND_LAT,o => o.AREA_NORTH_BOUND_LAT,o => o.AREA_WEST_BOUND_LON,o => o.AREA_EAST_BOUND_LON}},
				{"Coordinate_Operation Parameter Value",new List<Func<dynamic, dynamic>>{o => o.PARAMETER_VALUE}},
				{"Ellipsoid", new List<Func<dynamic, dynamic>>{o => o.SEMI_MAJOR_AXIS,o => o.INV_FLATTENING,o => o.SEMI_MINOR_AXIS}},
				{"Prime Meridian", new List<Func<dynamic, dynamic>>{o => o.GREENWICH_LONGITUDE}},
				{"Unit of Measure", new List<Func<dynamic, dynamic>>{o => o.FACTOR_B,o => o.FACTOR_C}},
			});

			// save the list of numbers to the DAT file
			using (var stream = File.Open(Path.Combine(outFolder, "numbers.dat"), FileMode.Create))
				NumbersToDatFile(epsgData.AllNumbers, stream);

			epsgData.Ellipsoids = connection.Query("SELECT * FROM [Ellipsoid]").ToList();
			epsgData.Areas = connection.Query("SELECT * FROM [Area]").ToList();

			using (var stream = File.Open(Path.Combine(outFolder, "ellipsoid.dat"), FileMode.Create))
				EllipsoidsToDatFile(epsgData, stream);

		}
		*/

		/*
		private static void EllipsoidsToDatFile(EpsgData epsgData, FileStream stream) {
			using (var writer = new BinaryWriter(stream)) {
				foreach (var ellipsoid in epsgData.Ellipsoids) {
					var major = (double) ellipsoid.SEMI_MAJOR_AXIS;
					var minor = null == ellipsoid.SEMI_MINOR_AXIS ? Double.NaN : (double) ellipsoid.SEMI_MINOR_AXIS;
					var invFlat = null == ellipsoid.INV_FLATTENING ? Double.NaN : (double) ellipsoid.INV_FLATTENING;

					writer.Write((ushort)ellipsoid.ELLIPSOID_CODE);
					writer.Write(epsgData.AllNumbers.IndexOf(major));
					writer.Write(epsgData.AllNumbers.IndexOf(Double.IsNaN(invFlat) ? minor : invFlat));
					writer.Write((byte)((int)ellipsoid.UOM_CODE - 9000));
					var textBytes = Encoding.UTF8.GetBytes(ellipsoid.ELLIPSOID_NAME as string ?? String.Empty);
					writer.Write((byte)textBytes.Length);
					writer.Write(textBytes);
				}
			}
		}

		private static void NumbersToDatFile(List<double> numbers, FileStream stream) {
			using (var writer = new BinaryWriter(stream)) {
				writer.Write((ushort)numbers.Count);
				foreach (var number in numbers) {
					writer.Write(number);
				}
			}
		}

		private static List<double> GenerateNumberValueLookup(IDbConnection connection, Dictionary<string, List<Func<dynamic, dynamic>>> numberSources) {
			// get all the strings we care about and break them into individual "words" to be used later
			var numberCounts = new Dictionary<double, int>();
			foreach (var table in numberSources) {
				var queryString = String.Format("Select * from [{0}]", table.Key);
				foreach (var queryFunc in table.Value) {
					foreach (var result in connection.Query(queryString)) {
						var data = queryFunc(result);
						if (data is double || data is float || data is int) {
							var number = (double) data;
							int numberCount;
							numberCounts[number] = numberCounts.TryGetValue(number, out numberCount)
								? numberCount + 1
								: 1;
						}
					}
				}
			}
			// more often used words go to the front of the list
			return numberCounts
				.OrderByDescending(e => e.Value)
				.Select(e => e.Key)
				.ToList();
		}

		private static void WordsToDatFile(List<string> words, FileStream stream) {
			using (var writer = new BinaryWriter(stream)) {
				writer.Write((ushort)words.Count);
				foreach (var word in words) {
					var textBytes = Encoding.UTF8.GetBytes(word);
					writer.Write((byte)textBytes.Length);
					writer.Write(textBytes);
				}
			}
		}

		private static List<string> GenerateWordLookup(IDbConnection connection, Dictionary<string, Func<dynamic, dynamic>> wordSources) {
			// get all the strings we care about and break them into individual "words" to be used later
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
			// more often used words go to the front of the list
			return wordCounts
				.OrderByDescending(e => e.Value)
				.Select(e => e.Key)
				.ToList();
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
		*/

	}
}
