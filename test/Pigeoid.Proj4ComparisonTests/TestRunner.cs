using Pigeoid.CoordinateOperation;
using Pigeoid.Core;
using Pigeoid.Epsg;
using Pigeoid.Interop.Proj4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.Proj4ComparisonTests
{
    public class TestRunner
    {

        public static EpsgCrs Crs4326 { get { return EpsgCrs.Get(4326); } }

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

        public static List<GeographicCoordinate> CreateTestPoints(IGeographicMbr mbr, int lonValueCount = 10, int latValueCount = 10) {
            var resultPoints = new List<GeographicCoordinate>(lonValueCount * latValueCount);
            var lonValues = GetValues(mbr.LongitudeRange, lonValueCount);
            var latValues = GetValues(mbr.LatitudeRange, latValueCount);
            for (int r = 0; r < latValues.Length; r++)
                for (int c = 0; c < lonValues.Length; c++)
                    resultPoints.Add(new GeographicCoordinate(latValues[r], lonValues[c]));
            return resultPoints;
        }

        private static double? GetEpsgAccuracy(ICoordinateOperationCrsPathInfo path) {
            double? sum = null;
            foreach (var op in path.CoordinateOperations) {
                EpsgCoordinateOperationInfoBase epsgOp;
                var epsgInverse = op as EpsgCoordinateOperationInverse;
                if (epsgInverse != null) {
                    epsgOp = epsgInverse.Core;
                }
                else {
                    epsgOp = op as EpsgCoordinateOperationInfoBase;
                    if (epsgOp == null)
                        continue;
                }

                var tx = epsgOp as EpsgCoordinateTransformInfo;
                if (tx != null) {
                    var accuracy = tx.Accuracy;
                    if (!Double.IsNaN(accuracy)) {
                        if (sum.HasValue) {
                            sum += accuracy;
                        }
                        else {
                            sum = accuracy;
                        }
                    }
                }
            }
            return sum;
        }

        private static HashSet<int> _restrictedOperationMethodCodes = new HashSet<int> {
            9613,
            9661,
            9658,
            9615,
            1025,
            9634,
            9662,
            1036,
            9614,
            1030,
            1040,
            1050,
            9655,
            9664,
            1050,
            1047,
            9620,
            1060,
            1048,
            9812, // dotspatial impl seems buggy
            9815 // dotspatial impl seems buggy
        };

        private static bool IsNotDeprecated(EpsgCrs crs) {
            return !crs.Deprecated;
        }

        private static bool IsNotDeprecated(ICoordinateOperationInfo op) {
            var epsgInverseOp = op as EpsgCoordinateOperationInverse;
            var epsgOp = epsgInverseOp != null
                ? epsgInverseOp.Core
                : op as EpsgCoordinateOperationInfoBase;
            return epsgOp == null || !epsgOp.Deprecated;
        }

        private static bool IsNotRestricted(ICoordinateOperationInfo op) {
            var epsgInverseOp = op as EpsgCoordinateOperationInverse;
            var epsgOp = epsgInverseOp != null
                ? epsgInverseOp.Core
                : op as EpsgCoordinateOperationInfoBase;

            if (epsgOp == null)
                return true;

            var catOp = epsgOp as EpsgConcatenatedCoordinateOperationInfo;
            if (catOp != null) {
                return catOp.Steps.All(s => !_restrictedOperationMethodCodes.Contains(s.Method.Code));
            }

            var epsgOpInfo = epsgOp as EpsgCoordinateOperationInfo;
            if(epsgOpInfo != null)
                return !_restrictedOperationMethodCodes.Contains(epsgOpInfo.Method.Code);

            return true;
        }

        public TestRunner() {
            _epsgPathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
            _epsgPathGenerator.CrsFilters = new List<Predicate<EpsgCrs>> {
                IsNotDeprecated
            };
            _epsgPathGenerator.OpFilters = new List<Predicate<ICoordinateOperationInfo>>{
                IsNotDeprecated,
                IsNotRestricted
            };
            _coordinateOperationCompiler = new StaticCoordinateOperationCompiler();
        }

        private readonly EpsgCrsCoordinateOperationPathGenerator _epsgPathGenerator;
        private readonly StaticCoordinateOperationCompiler _coordinateOperationCompiler;

        public IEnumerable<EpsgCrs> GetAllCrs() {
            return EpsgCrs.Values.Where(IsNotDeprecated);
        }

        private class TestableCrs
        {
            public EpsgCrs Crs;
            public ITransformation From4325Transform;
            public EpsgArea Area;
        }

        public IEnumerable<TestCase> CreateTestCases() {
            var crs4326 = Crs4326;
            if (crs4326 == null)
                throw new InvalidOperationException("No implementation of EPSG:4326 has been found.");

            var testableCrsList = new List<TestableCrs>();
            foreach (var crs in GetAllCrs().Where(x => x.Kind == EpsgCrsKind.Projected)) {
                if (crs.Area == null)
                    continue;

                var paths = _epsgPathGenerator.Generate(crs4326, crs)
                    .OrderBy(x => GetEpsgAccuracy(x) ?? 999);
                var transform = paths
                    .Select(p => _coordinateOperationCompiler.Compile(p))
                    .Where(x => x != null)
                    .FirstOrDefault();
                if (transform == null)
                    continue;

                Console.WriteLine("Preparing {1}: {0} ", crs, testableCrsList.Count);

                testableCrsList.Add(new TestableCrs {
                    Crs = crs,
                    From4325Transform = transform,
                    Area = crs.Area
                });
            }

            Console.WriteLine("Prepared tests for {0} CRSs.", testableCrsList.Count);

            for (var fromTestableIndex = 0; fromTestableIndex < testableCrsList.Count; ++fromTestableIndex){
                var fromTestable = testableCrsList[fromTestableIndex];
                var fromCrs = fromTestable.Crs;
                var fromArea = fromTestable.Area;

                Console.WriteLine("{0:P} complete...", (fromTestableIndex / (double)testableCrsList.Count));

                foreach (var toTestable in testableCrsList) {
                    if (fromTestable == toTestable)
                        continue;

                    var toCrs = toTestable.Crs;
                    var toArea = toTestable.Area;

                    var areaIntersection = fromArea.Intersection(toArea);
                    if (areaIntersection == null)
                        continue;

                    var inputCoordsWgs84 = CreateTestPoints(areaIntersection);
                    var transformedInputCoords = inputCoordsWgs84.ConvertAll(c => fromTestable.From4325Transform.TransformValue(c));

                    yield return new TestCase {
                        Source = fromCrs,
                        Target = toCrs,
                        Area = areaIntersection,
                        InputWgs84Coordinates = inputCoordsWgs84,
                        InputCoordinates = transformedInputCoords
                    };
                }
            }
        }

        public TestResult Execute(TestCase testCase) {
            var result = new TestResult {
                Source = testCase.Source,
                Target = testCase.Target
            };

            var staticResult = ExecuteStaticEpsg(testCase);
            var proj4Results = ExecuteProj4(testCase);
            try {
                var statsData = new CoordinateComparisonStatistics();
                statsData.Process(proj4Results.ResultData, staticResult.ResultData);
                result.StatsData = statsData;
            }
            catch (Exception ex) {
                ; // eat it
            }

            return result;
        }

        private ConversionResult ExecuteStaticEpsg(TestCase testCase) {
            var result = new ConversionResult();
            try {

                var paths = _epsgPathGenerator.Generate(testCase.Source, testCase.Target)
                    .OrderBy(x => GetEpsgAccuracy(x) ?? 999);

                var compiler = new StaticCoordinateOperationCompiler();
                var compiledPaths = paths
                    .Select(x => compiler.Compile(x));

                var bestCompiledPath = compiledPaths.FirstOrDefault(x => x != null);

                var transformation = bestCompiledPath;
                if (transformation == null)
                    throw new InvalidOperationException("No compiled transformation");

                result.ResultData = transformation.TransformValues(testCase.InputCoordinates).ToArray();
            }
            catch (Exception ex) {
                result.Exception = ex;
            }

            return result;
        }

        private ConversionResult ExecuteProj4(TestCase testCase) {
            var result = new ConversionResult();
            try {
                var transformation = new Proj4Transform(testCase.Source, testCase.Target);
                if(transformation == null)
                    throw new InvalidOperationException("No transformation");

                result.ResultData = transformation.TransformValues(testCase.InputCoordinates).ToArray();
            }
            catch (Exception ex) {
                result.Exception = ex;
            }
            return result;
        }


    }
}
