using System;
using Pigeoid.Epsg;
using System.Collections.Generic;
using System.Linq;

namespace Pigeoid.CoordinateOpFullTester
{
    class Program
    {

        static IEnumerable<CoordinateOpTestCase> CreateTestCases() {
            var items = EpsgCrs.Values.Where(x => !x.Deprecated);

            // ReSharper disable PossibleMultipleEnumeration
            foreach (var fromCrs in items) {
                var fromArea = fromCrs.Area;
                foreach (var toCrs in items) {
                    if (toCrs != fromCrs && fromArea.Intersects(toCrs.Area)) {
                        yield return new CoordinateOpTestCase(fromCrs, toCrs);
                    }
                }
            }
            foreach (var item in items) {
                yield return new CoordinateOpTestCase(item, item);
            }
            // ReSharper restore PossibleMultipleEnumeration
        } 

        static void Main(string[] args) {
            //var testCaseCount = CreateTestCases().Count();
            /*var count = 0;
            foreach (var testCase in CreateTestCases()) {
                count++;
                //Console.WriteLine("{0}:\t{1}", count, testCase);
            }*/
            //Console.WriteLine(testCaseCount);

            foreach (var testCase in CreateTestCases()) {
                var intersectingArea = testCase.IntersectingArea;
                var testPoints = testCase.CreateTestPoints(intersectingArea).ToArray();
                Console.WriteLine("{0}", testCase);
            }

            Console.WriteLine("Done...");
            Console.ReadKey();

        }
    }
}
