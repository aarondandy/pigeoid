using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.CoordinateOperation;
using Pigeoid.Core;
using Pigeoid.Epsg;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOpFullTester
{
    public class CoordOpTester
    {

        public static EpsgCrs Crs4326 { get { return EpsgMicroDatabase.Default.GetCrs(4326); } }

        public static double[] GetValues(double start, double end, int count) {
            var result = new double[count];
            var lastIndex = count - 1;
            var distance = end - start;
            for (int i = 1; i < lastIndex; i++) {
                var ratio = i / (double)(lastIndex);
                result[i] = start + (ratio * distance);
            }
            result[0] = start;
            result[lastIndex] = end;
            return result;
        }

        public static double[] GetValues(LongitudeDegreeRange range, int count) {
            var distance = range.GetMagnitude();
            var result = GetValues(range.Start, range.Start + distance, count);
            for (int i = 0; i < result.Length; i++)
                result[i] = LongitudeDegreeRange.DefaultPeriodicOperations.Fix(result[i]);

            return result;

        }

        public static double[] GetValues(IPeriodicRange<double> range, int count) {
            if (range is LongitudeDegreeRange)
                return GetValues((LongitudeDegreeRange)range, count);
            throw new NotImplementedException();
        }

        public static double[] GetValues(Range range, int count) {
            return GetValues(range.Low, range.High, count);
        }

        public static IEnumerable<GeographicCoordinate> CreateTestPoints(IGeographicMbr mbr, int lonValueCount = 10, int latValueCount = 10) {
            var lonValues = GetValues(mbr.LongitudeRange, lonValueCount);
            var latValues = GetValues(mbr.LatitudeRange, latValueCount);
            for (int r = 0; r < latValues.Length; r++)
                for (int c = 0; c < lonValues.Length; c++)
                    yield return new GeographicCoordinate(latValues[r], lonValues[c]);
        }

        [Obsolete]
        private Type GetCoordinateType(ICrs crs) {
            if (crs is ICrsGeocentric)
                return typeof(Point3);
            if (crs is ICrsGeographic)
                return typeof(GeographicCoordinate);
            if (crs is ICrsProjected)
                return typeof(Point2);
            return null;
        }

        public CoordOpTester() {
            /*_epsgPathGenerator = new EpsgCrsCoordinateOperationPathGenerator(
                new EpsgCrsCoordinateOperationPathGeneratorOld.SharedOptionsAreaPredicate(x => !x.Deprecated, x => !x.Deprecated));*/
            var _epsgPathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
            _coordinateOperationCompiler = new StaticCoordinateOperationCompiler();
        }

        private readonly EpsgCrsCoordinateOperationPathGenerator _epsgPathGenerator;
        private readonly StaticCoordinateOperationCompiler _coordinateOperationCompiler;
        
        public IEnumerable<ICrs> GetAllCrs() {
            return EpsgMicroDatabase.Default.GetAllCrs().Where(x => !x.Deprecated);
        }

        private IGeographicMbr ExtractMbr(ICrs crs) {
            var epsgCrs = crs as EpsgCrs;
            if (epsgCrs != null)
                return epsgCrs.Area;
            return null;
        }

        public IEnumerable<CoordOpTestCase> CreateTestCases() {
            var crs4326 = Crs4326;
            if(crs4326 == null)
                throw new InvalidOperationException("No implementation of EPSG:4326 has been found.");

            var testableCrsList = GetAllCrs()
                .Where(x => x is ICrsGeocentric || x is ICrsGeographic || x is ICrsProjected)
                .ToList();

            var from4326 = testableCrsList
                .Select(crs => CreateOperationPath(crs4326, crs))
                .Where(x => x != null);

            foreach (var from4326Path in from4326) {
                var fromCrs = from4326Path.To;

                var fromArea = ExtractMbr(fromCrs);
                if (fromArea == null)
                    continue;

                ITransformation from4326Transformation;
                try {
                    from4326Transformation = _coordinateOperationCompiler.Compile(from4326Path);
                }
                catch {
                    Console.WriteLine("EPSG:4326 Transformation failed for {0}", fromCrs);
                    continue;
                }

                if(from4326Transformation == null)
                    continue;

                foreach (var toCrs in testableCrsList) {
                    if (toCrs == fromCrs)
                        continue;

                    var toArea = ExtractMbr(toCrs);
                    if(toArea == null)
                        continue;
                    
                    var areaIntersection = fromArea.Intersection(toArea);
                    if(areaIntersection == null)
                        continue;

                    var path = CreateOperationPath(fromCrs, toCrs);
                    if(path == null)
                        continue;

                    var inputCoordsWgs84 = CreateTestPoints(areaIntersection).ToList();
                    var transformedInputCoords = inputCoordsWgs84
                        .Select(x => from4326Transformation.TransformValue(x))
                        .ToList();

                    yield return new CoordOpTestCase {
                        From = fromCrs,
                        To = toCrs,
                        Area = areaIntersection,
                        Path = path,
                        InputWgs84Coordinates = inputCoordsWgs84,
                        InputCoordinates = transformedInputCoords
                    };
                }
            }
        }

        public ICoordinateOperationCrsPathInfo CreateOperationPath(ICrs from, ICrs to) {
            throw new NotImplementedException();
            //return _epsgPathGenerator.Generate(from, to);
        }
    }
}
