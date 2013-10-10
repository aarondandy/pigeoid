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

        public static double[] GetValues(double start, double end, int count) {
            var result = new double[count];
            var lastIndex = count - 1;
            var distance = end - start;
            for (int i = 1; i < lastIndex; i++) {
                var ratio = i / (double)(lastIndex);
                result[i] = ratio * distance;
            }
            result[0] = start;
            result[lastIndex] = end;
            return result;
        }

        public static double[] GetValues(LongitudeDegreeRange range, int count) {
            return GetValues(range.Start, range.End, count);
        }

        public static double[] GetValues(Range range, int count) {
            return GetValues(range.Low, range.High, count);
        }

        public static IEnumerable<GeographicCoordinate> CreateTestPoints(IGeographicMbr mbr, int lonValueCount = 10, int latValueCount = 10) {
            var lonValues = GetValues(mbr.LongitudeRange.Start, mbr.LongitudeRange.End, lonValueCount);
            var latValues = GetValues(mbr.LatitudeRange.Low, mbr.LatitudeRange.High, latValueCount);
            for (int r = 0; r < latValues.Length; r++)
                for (int c = 0; c < lonValues.Length; c++)
                    yield return new GeographicCoordinate(latValues[r], lonValues[c]);
        }

        public static EpsgCrs Crs4326 { get { return EpsgCrs.Get(4326); } }

        public CoordOpTester() {
            _epsgPathGenerator = new EpsgCrsCoordinateOperationPathGenerator(
                new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(x => !x.Deprecated, x => !x.Deprecated));
            _coordinateOperationCompiler = new StaticCoordinateOperationCompiler();
        }

        private readonly EpsgCrsCoordinateOperationPathGenerator _epsgPathGenerator;
        private readonly StaticCoordinateOperationCompiler _coordinateOperationCompiler;
        
        public IEnumerable<ICrs> GetAllCrs() {
            return EpsgCrs.Values.Where(x => !x.Deprecated);
        }

        private IGeographicMbr ExtractMbr(ICrs crs) {
            var epsgCrs = crs as EpsgCrs;
            if (epsgCrs != null)
                return epsgCrs.Area;
            return null;
        }

        private Type GetCoordinateType(ICrs crs) {
            if (crs is ICrsGeocentric)
                return typeof (Point3);
            if (crs is ICrsGeographic)
                return typeof (GeographicCoordinate);
            if (crs is ICrsProjected)
                return typeof (Point2);
            return null;
        }

        public IEnumerable<CoordOpTestCase> CreateTestCases() {
            var crs4326 = Crs4326;
            if(crs4326 == null)
                throw new InvalidOperationException("No implementation of EPSG:4326 has been found.");

            var from4326 = GetAllCrs()
                .Select(crs => CreateOperationPath(crs4326, crs))
                .Where(x => x != null);

            var allOtherCrs = GetAllCrs().ToList();

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

                foreach (var toCrs in allOtherCrs) {
                    var toArea = ExtractMbr(toCrs);
                    if(toArea == null)
                        continue;

                    var areaIntersection = fromArea.Intersection(toArea);
                    if(areaIntersection == null)
                        continue;

                    var path = CreateOperationPath(fromCrs, toCrs);
                    if(path == null)
                        continue;
                    
                    var testPoints4326 = CreateTestPoints(areaIntersection);
                    var fromCrsCoordType = GetCoordinateType(fromCrs);

                    throw new NotImplementedException("Need a simpler way to apply an ITransformation");

                    yield return new CoordOpTestCase {
                        From = fromCrs,
                        To = toCrs,
                        Area = areaIntersection,
                        Path = path
                    };
                }
            }
        }

        public ICoordinateOperationCrsPathInfo CreateOperationPath(ICrs from, ICrs to) {
            return _epsgPathGenerator.Generate(from, to);
        }

    }
}
