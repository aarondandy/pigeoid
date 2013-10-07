using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Pigeoid.CoordinateOperation;

namespace Pigeoid.Epsg.ProjectionTest
{
    public class CrsTest
    {

        public CrsTest() {
            IgnoreDeprecated = true;
        }

        public bool IgnoreDeprecated { get; private set; }

        public IEnumerable<CrsTestCase> GenerateAreaIntersectingCrs() {
            var items = EpsgCrs.Values;
            if (IgnoreDeprecated)
                items = items.Where(x => !x.Deprecated);

            // ReSharper disable PossibleMultipleEnumeration
            foreach (var fromCrs in items) {
                var fromArea = fromCrs.Area;
                foreach (var toCrs in items) {
                    if (toCrs != fromCrs && fromArea.Intersects(toCrs.Area)) {
                        yield return new CrsTestCase(fromCrs, toCrs);
                    }
                }
            }
            // ReSharper restore PossibleMultipleEnumeration
        }

        public void Run(Action<IEnumerable<CrsTestCase>> saveResults) {
            var crsTests = GenerateAreaIntersectingCrs();
            var graphFilter = IgnoreDeprecated
                ? new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(x => !x.Deprecated, x => !x.Deprecated)
                : new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(x => true, x => true);

            var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator(graphFilter);
            var transformGenerator = new StaticCoordinateOperationCompiler();

            throw new NotImplementedException();
        }


    }
}
