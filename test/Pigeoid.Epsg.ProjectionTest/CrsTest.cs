using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg.ProjectionTest
{
	public class CrsTest
	{

		public IEnumerable<CrsTestCase> GenerateAreaIntersectingCrs() {
			var items = EpsgCrs.Values.ToList();
			foreach (var fromCrs in items) {
				var fromArea = fromCrs.Area;
				foreach (var toCrs in items) {
					if (toCrs != fromCrs && fromArea.Intersects(toCrs.Area)) {
						yield return new CrsTestCase(fromCrs,toCrs);
					}
				}
			}
		}

		public void Run() {
			Console.Write("Generating test cases...");
			var crsTestList = GenerateAreaIntersectingCrs().ToList();
			Console.WriteLine("done.");

			Console.WriteLine("Beginning Tests");

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var generator = new EpsgCrsCoordinateOperationPathGenerator();
			int testNumber = 0;
			Parallel.ForEach(crsTestList, testCase => {
				var progress = Interlocked.Increment(ref testNumber);
				if (progress % 100 == 0) {
					Console.WriteLine(
						"{0:P6} ({1}/{2}) @{3}ops/s",
						(progress / (double)crsTestList.Count),
						progress,
						crsTestList.Count,
						progress / (stopwatch.ElapsedMilliseconds / 1000.0)
					);
				}
				try {
					testCase.Run(generator);
				}
				catch(Exception ex) {
					Console.WriteLine("Failure on '{0}'.", testCase);
					Console.WriteLine(ex);
				}
			});

			stopwatch.Stop();
			Console.WriteLine("All tests complete");
			Console.WriteLine("Execution took: {0}", stopwatch.Elapsed);
		}


	}
}
