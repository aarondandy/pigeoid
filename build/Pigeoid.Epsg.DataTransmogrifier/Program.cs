using System;
using System.Collections.Generic;
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

		private static IEnumerable<T> Concat<T>(params IEnumerable<T>[] itemLists) {
			foreach (var itemList in itemLists) {
				foreach (var item in itemList) {
					yield return item;
				}
			}
		}

		private static IEnumerable<string> ExtractStrings<T>(IEnumerable<T> objects, params Func<T, string>[] extractions) {
			foreach (var item in objects) {
				foreach (var extraction in extractions) {
					var extractedValue = extraction(item);
					if (null != extractedValue)
						yield return extractedValue;
				}
			}
		}

		private static IEnumerable<double> ExtractDoubles<T>(IEnumerable<T> objects, params Func<T, double>[] extractions) {
			foreach (var item in objects) {
				foreach (var extraction in extractions) {
					var extractedValue = extraction(item);
					if (!Double.IsNaN(extractedValue))
						yield return extractedValue;
				}
			}
		}

		private static IEnumerable<double> ExtractDoubles<T>(IEnumerable<T> objects, params Func<T, double?>[] extractions) {
			foreach (var item in objects) {
				foreach (var extraction in extractions) {
					var extractedValue = extraction(item);
					if (extractedValue.HasValue && !Double.IsNaN(extractedValue.Value))
						yield return extractedValue.Value;
				}
			}
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

			var outFolder = Path.Combine(dataFolderPath, "out");
			if (!Directory.Exists(outFolder))
				Directory.CreateDirectory(outFolder);

			using(var session = sessionFactory.OpenSession()) {
				var epsgData = new EpsgData(session);

				epsgData.WordLookupList = StringUtils.BuildWordCountLookup(Concat(
						//ExtractStrings(epsgData.Aliases, o => o.Alias),
						ExtractStrings(epsgData.Areas, o => o.Name /*, o => StringUtils.TrimTrailingPeriod(o.AreaOfUse)*/ /*,o => o.Iso2*/ /*,o => o.Iso3*/),
						ExtractStrings(epsgData.Axes, o => o.Name, o => o.Orientation, o => o.Abbreviation),
						ExtractStrings(epsgData.Crs, o => o.Name),
						ExtractStrings(epsgData.CoordinateSystems, o => o.Name),
						ExtractStrings(epsgData.CoordinateOperations, o => o.Name),
						ExtractStrings(epsgData.CoordinateOperationMethods, o => o.Name),
						ExtractStrings(epsgData.Parameters, o => o.Name , o => o.Description),
						ExtractStrings(epsgData.ParamValues, o => o.TextValue),
						ExtractStrings(epsgData.Datums, o => o.Name),
						ExtractStrings(epsgData.Ellipsoids, o => o.Name),
						ExtractStrings(epsgData.PrimeMeridians, o => o.Name),
						ExtractStrings(epsgData.Uoms, o => o.Name)
						//ExtractStrings(epsgData.NamingSystems, o => o.Name)
					).SelectMany(StringUtils.BreakIntoWordParts))
					.OrderByDescending(o => o.Value)
					.Select(o => o.Key)
					.ToList();

				using (var streamIndex = File.Open(Path.Combine(outFolder, "words.dat"), FileMode.Create))
				using (var writerIndex = new BinaryWriter(streamIndex))
				using (var streamText = File.Open(Path.Combine(outFolder, "words.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteWordLookup(epsgData, writerText, writerIndex);

				epsgData.SetNumberLists(NumberUtils.BuildNumberCountLookup(Concat(
						//ExtractDoubles(epsgData.Areas, o => o.EastBound, o => o.WestBound, o => o.SouthBound, o => o.NorthBound),
						ExtractDoubles(epsgData.CoordinateOperations, o => o.Accuracy),
						ExtractDoubles(epsgData.Ellipsoids, o => o.SemiMajorAxis, o => o.SemiMinorAxis, o => o.InverseFlattening),
						ExtractDoubles(epsgData.ParamValues, o => o.NumericValue),
						ExtractDoubles(epsgData.PrimeMeridians, o => o.GreenwichLon),
						ExtractDoubles(epsgData.Uoms, o => o.FactorB, o => o.FactorC)
					))
					.OrderByDescending(o => o.Value)
					.Select(o => o.Key)
				);

				using (var streamDouble = File.Open(Path.Combine(outFolder, "numbersd.dat"), FileMode.Create))
				using (var writerDouble = new BinaryWriter(streamDouble))
				using (var streamInt = File.Open(Path.Combine(outFolder, "numbersi.dat"), FileMode.Create))
				using (var writerInt = new BinaryWriter(streamInt))
				using (var streamShort = File.Open(Path.Combine(outFolder, "numberss.dat"), FileMode.Create))
				using (var writerShort = new BinaryWriter(streamShort))
					WriterUtils.WriteNumberLookups(epsgData, writerDouble, writerInt, writerShort);

				using (var streamData = File.Open(Path.Combine(outFolder, "areas.dat"), FileMode.Create))
				using (var writerData = new BinaryWriter(streamData))
				using (var streamText = File.Open(Path.Combine(outFolder, "areas.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
				using (var streamIso2 = File.Open(Path.Combine(outFolder, "iso2.dat"), FileMode.Create))
				using (var writerIso2 = new BinaryWriter(streamIso2))
				using (var streamIso3 = File.Open(Path.Combine(outFolder, "iso3.dat"), FileMode.Create))
				using (var writerIso3 = new BinaryWriter(streamIso3))
					WriterUtils.WriteAreas(epsgData, writerData, writerText, writerIso2, writerIso3);

				using (var streamData = File.Open(Path.Combine(outFolder, "axis.dat"), FileMode.Create))
				using (var writerData = new BinaryWriter(streamData))
				using (var streamText = File.Open(Path.Combine(outFolder, "axis.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteAxes(epsgData, writerData, writerText);

				using (var streamData = File.Open(Path.Combine(outFolder, "ellipsoids.dat"), FileMode.Create))
				using (var writerData = new BinaryWriter(streamData))
				using (var streamText = File.Open(Path.Combine(outFolder, "ellipsoids.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteEllipsoids(epsgData, writerData, writerText);

				using (var streamData = File.Open(Path.Combine(outFolder, "meridians.dat"), FileMode.Create))
				using (var writerData = new BinaryWriter(streamData))
				using (var streamText = File.Open(Path.Combine(outFolder, "meridians.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteMeridians(epsgData, writerData, writerText);

				using (var streamDataEgr = File.Open(Path.Combine(outFolder, "datumegr.dat"), FileMode.Create))
				using (var writerDataEgr = new BinaryWriter(streamDataEgr))
				using (var streamDataVer = File.Open(Path.Combine(outFolder, "datumver.dat"), FileMode.Create))
				using (var writerDataVer = new BinaryWriter(streamDataVer))
				using (var streamDataGeo = File.Open(Path.Combine(outFolder, "datumgeo.dat"), FileMode.Create))
				using (var writerDataGeo = new BinaryWriter(streamDataGeo))
				using (var streamText = File.Open(Path.Combine(outFolder, "datums.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteDatums(epsgData, writerDataEgr, writerDataVer, writerDataGeo, writerText);

				using (var streamDataLen = File.Open(Path.Combine(outFolder, "uomlen.dat"), FileMode.Create))
				using (var writerDataLen = new BinaryWriter(streamDataLen))
				using (var streamDataAng = File.Open(Path.Combine(outFolder, "uomang.dat"), FileMode.Create))
				using (var writerDataAng = new BinaryWriter(streamDataAng))
				using (var streamDataScl = File.Open(Path.Combine(outFolder, "uomscl.dat"), FileMode.Create))
				using (var writerDataScl = new BinaryWriter(streamDataScl))
				using (var streamText = File.Open(Path.Combine(outFolder, "uoms.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteUoms(epsgData, writerDataLen, writerDataAng, writerDataScl, writerText);

				using (var streamData = File.Open(Path.Combine(outFolder, "parameters.dat"), FileMode.Create))
				using (var writerData = new BinaryWriter(streamData))
				using (var streamText = File.Open(Path.Combine(outFolder, "parameters.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteParameters(epsgData, writerData, writerText);

				using (var streamData = File.Open(Path.Combine(outFolder, "opmethod.dat"), FileMode.Create))
				using (var writerData = new BinaryWriter(streamData))
				using (var streamText = File.Open(Path.Combine(outFolder, "opmethod.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteOpMethod(epsgData, writerData, writerText);

				using (var streamData = File.Open(Path.Combine(outFolder, "coordsys.dat"), FileMode.Create))
				using (var writerData = new BinaryWriter(streamData))
				using (var streamText = File.Open(Path.Combine(outFolder, "coordsys.txt"), FileMode.Create))
				using (var writerText = new BinaryWriter(streamText))
					WriterUtils.WriteCoordSystems(epsgData, writerData, writerText);

			}

		}

	}
}
