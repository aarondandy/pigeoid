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
