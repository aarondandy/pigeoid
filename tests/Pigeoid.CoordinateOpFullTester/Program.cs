using System;
using Pigeoid.Epsg;
using System.Collections.Generic;
using System.Linq;

namespace Pigeoid.CoordinateOpFullTester
{
    class Program
    {

        static void Main(string[] args) {
            var tester = new CoordOpTester();

            foreach (var testCase in tester.CreateTestCases()) {
                testCase.Execute();
                //var intersectingArea = testCase.Area;
                //var testPoints = CoordOpTester.CreateTestPoints(intersectingArea).ToArray();
                Console.WriteLine("{0}", testCase);
            }

            Console.WriteLine("Done...");
            Console.ReadKey();

        }
    }
}
