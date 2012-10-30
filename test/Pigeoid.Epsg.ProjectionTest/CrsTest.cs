using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

		public IEnumerable<T[]> Batch<T>(List<T> items, int batchSize){
			for(int i = 0; i < items.Count; i+=batchSize){
				int startIndex = i;
				int endIndex = startIndex + batchSize;
				if(endIndex > items.Count){
					endIndex = items.Count;
				}
				var batch = new T[endIndex - startIndex];
				if(batch.Length > 0){
					items.CopyTo(startIndex, batch, 0, batch.Length);
					yield return batch;
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

			int processedItems = 0;
			foreach (var batch in Batch(crsTestList, 512)){
				Console.WriteLine(
					"{0:P6} ({1}/{2}) @{3}ops/s",
					(processedItems / (double)crsTestList.Count),
					processedItems,
					crsTestList.Count,
					processedItems / (stopwatch.ElapsedMilliseconds / 1000.0)
				);
				Parallel.ForEach(batch, testCase =>{
					try {
						testCase.Run(generator);
					}
					catch (Exception ex) {
						Console.WriteLine("Failure on '{0}'.", testCase);
						Console.WriteLine(ex);
					}
				});
				processedItems += batch.Length;
			}

			stopwatch.Stop();
			Console.WriteLine("All tests complete");
			Console.WriteLine("Execution took: {0}", stopwatch.Elapsed);
		}


	}
}
