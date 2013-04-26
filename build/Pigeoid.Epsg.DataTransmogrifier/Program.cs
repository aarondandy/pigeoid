using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            return itemLists.SelectMany(itemList => itemList);
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
                dataFolderPath = Path.GetFullPath(args[0]);

            var alternatePath = @"../../build/data";
            if (Directory.Exists(dataFolderPath))
                ;
            else if (Directory.Exists(alternatePath))
                dataFolderPath = alternatePath;
            else
                throw new IOException("Folder does not exist: " + dataFolderPath);

            var dataFilePath = GetDatabasePath(dataFolderPath);
            if (null == dataFilePath || !File.Exists(dataFilePath))
                throw new FileNotFoundException("Database file not found.");
            Console.WriteLine("Found database: " + dataFilePath);

            var outFolder = Path.Combine(dataFolderPath, "out");
            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            using (var repository = new EpsgRepository(new FileInfo(dataFilePath))) {
                var epsgData = new EpsgData(repository);

                epsgData.WordLookUpList = StringUtils.BuildWordCountLookUp(Concat(
                    //ExtractStrings(epsgData.Aliases, o => o.Alias),
                        ExtractStrings(epsgData.Repository.Areas, o => o.Name /*, o => StringUtils.TrimTrailingPeriod(o.AreaOfUse)*/ /*,o => o.Iso2*/ /*,o => o.Iso3*/),
                        ExtractStrings(epsgData.Repository.Axes, o => o.Name, o => o.Orientation, o => o.Abbreviation),
                        ExtractStrings(epsgData.Repository.Crs, o => o.Name),
                        ExtractStrings(epsgData.Repository.CoordinateSystems, o => o.Name),
                        ExtractStrings(epsgData.Repository.CoordinateOperations, o => o.Name),
                        ExtractStrings(epsgData.Repository.CoordinateOperationMethods, o => o.Name),
                        ExtractStrings(epsgData.Repository.Parameters, o => o.Name, o => o.Description),
                        ExtractStrings(epsgData.Repository.ParamValues, o => o.TextValue),
                        ExtractStrings(epsgData.Repository.Datums, o => o.Name),
                        ExtractStrings(epsgData.Repository.Ellipsoids, o => o.Name),
                        ExtractStrings(epsgData.Repository.PrimeMeridians, o => o.Name),
                        ExtractStrings(epsgData.Repository.Uoms, o => o.Name)
                    //ExtractStrings(epsgData.NamingSystems, o => o.Name)
                    ).SelectMany(StringUtils.BreakIntoWordParts))
                    .OrderByDescending(o => o.Value)
                    .Select(o => o.Key)
                    .ToList();

                using (var streamIndex = File.Open(Path.Combine(outFolder, "words.dat"), FileMode.Create))
                using (var writerIndex = new BinaryWriter(streamIndex))
                using (var streamText = File.Open(Path.Combine(outFolder, "words.txt"), FileMode.Create))
                using (var writerText = new BinaryWriter(streamText))
                    WriterUtils.WriteWordLookUp(epsgData, writerText, writerIndex);

                epsgData.SetNumberLists(NumberUtils.BuildNumberCountLookUp(Concat(
                    //ExtractDoubles(epsgData.Areas, o => o.EastBound, o => o.WestBound, o => o.SouthBound, o => o.NorthBound),
                        ExtractDoubles(epsgData.Repository.CoordinateOperations, o => o.Accuracy),
                        ExtractDoubles(epsgData.Repository.Ellipsoids, o => o.SemiMajorAxis, o => o.SemiMinorAxis, o => o.InverseFlattening),
                        ExtractDoubles(epsgData.Repository.ParamValues, o => o.NumericValue),
                        ExtractDoubles(epsgData.Repository.PrimeMeridians, o => o.GreenwichLon),
                        ExtractDoubles(epsgData.Repository.Uoms, o => o.FactorB, o => o.FactorC)
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
                    WriterUtils.WriteNumberLookUps(epsgData, writerDouble, writerInt, writerShort);

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

                using (var streamDataEngineering = File.Open(Path.Combine(outFolder, "datumegr.dat"), FileMode.Create))
                using (var writerDataEngineering = new BinaryWriter(streamDataEngineering))
                using (var streamDataVertical = File.Open(Path.Combine(outFolder, "datumver.dat"), FileMode.Create))
                using (var writerDataVertical = new BinaryWriter(streamDataVertical))
                using (var streamDataGeo = File.Open(Path.Combine(outFolder, "datumgeo.dat"), FileMode.Create))
                using (var writerDataGeo = new BinaryWriter(streamDataGeo))
                using (var streamText = File.Open(Path.Combine(outFolder, "datums.txt"), FileMode.Create))
                using (var writerText = new BinaryWriter(streamText))
                    WriterUtils.WriteDatums(epsgData, writerDataEngineering, writerDataVertical, writerDataGeo, writerText);

                using (var streamDataLength = File.Open(Path.Combine(outFolder, "uomlen.dat"), FileMode.Create))
                using (var writerDataLength = new BinaryWriter(streamDataLength))
                using (var streamDataAngle = File.Open(Path.Combine(outFolder, "uomang.dat"), FileMode.Create))
                using (var writerDataAngle = new BinaryWriter(streamDataAngle))
                using (var streamDataScale = File.Open(Path.Combine(outFolder, "uomscl.dat"), FileMode.Create))
                using (var writerDataScale = new BinaryWriter(streamDataScale))
                using (var streamText = File.Open(Path.Combine(outFolder, "uoms.txt"), FileMode.Create))
                using (var writerText = new BinaryWriter(streamText))
                    WriterUtils.WriteUnitOfMeasures(epsgData, writerDataLength, writerDataAngle, writerDataScale, writerText);

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
                    WriterUtils.WriteCoordinateSystems(epsgData, writerData, writerText);

                using (var streamText = File.Open(Path.Combine(outFolder, "params.txt"), FileMode.Create))
                using (var writerText = new BinaryWriter(streamText))
                    WriterUtils.WriteParamData(
                        epsgData, writerText,
                        code => new BinaryWriter(
                            File.Open(
                                Path.Combine(outFolder, String.Format("param{0}.dat", code)),
                                FileMode.Create
                            )
                        )
                    );

                using (var streamDataProjection = File.Open(Path.Combine(outFolder, "crsprj.dat"), FileMode.Create))
                using (var writerDataProjection = new BinaryWriter(streamDataProjection))
                using (var streamDataComposite = File.Open(Path.Combine(outFolder, "crscmp.dat"), FileMode.Create))
                using (var writerDataComposite = new BinaryWriter(streamDataComposite))
                using (var streamDataGeo = File.Open(Path.Combine(outFolder, "crsgeo.dat"), FileMode.Create))
                using (var writerDataGeo = new BinaryWriter(streamDataGeo))
                using (var streamText = File.Open(Path.Combine(outFolder, "crs.txt"), FileMode.Create))
                using (var writerText = new BinaryWriter(streamText))
                    WriterUtils.WriteCoordinateReferenceSystem(epsgData, writerText, writerDataProjection, writerDataGeo, writerDataComposite);

                using (var streamDataConversion = File.Open(Path.Combine(outFolder, "opconv.dat"), FileMode.Create))
                using (var writerDataConversion = new BinaryWriter(streamDataConversion))
                using (var streamDataConcat = File.Open(Path.Combine(outFolder, "opcat.dat"), FileMode.Create))
                using (var writerDataConcat = new BinaryWriter(streamDataConcat))
                using (var streamDataTransform = File.Open(Path.Combine(outFolder, "optran.dat"), FileMode.Create))
                using (var writerDataTransform = new BinaryWriter(streamDataTransform))
                using (var streamDataPath = File.Open(Path.Combine(outFolder, "oppath.dat"), FileMode.Create))
                using (var writerDataPath = new BinaryWriter(streamDataPath))
                using (var streamText = File.Open(Path.Combine(outFolder, "op.txt"), FileMode.Create))
                using (var writerText = new BinaryWriter(streamText))
                    WriterUtils.WriteCoordinateOperations(epsgData, writerText, writerDataConversion, writerDataTransform, writerDataConcat, writerDataPath);

            }

        }

    }
}
