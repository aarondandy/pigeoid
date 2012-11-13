using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Pigeoid.CoordinateOperationCompilation;

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

			var itemsList = items.ToList();
			foreach (var fromCrs in itemsList) {
				var fromArea = fromCrs.Area;
				foreach (var toCrs in itemsList) {
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

		public void Run(Action<IEnumerable<CrsTestCase>> saveResults) {
			var batchBufferSize = 8;
			var batchSize = 512;

			Console.Write("Generating test cases...");
			var crsTestList = GenerateAreaIntersectingCrs().ToList();
			Console.WriteLine("done.");

			Console.WriteLine("Beginning Tests");

			var executedTestsBuffer = new BlockingCollection<CrsTestCase[]>(batchBufferSize);
			var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
			var writeResultsTask = taskFactory.StartNew(() => saveResults(executedTestsBuffer.GetConsumingEnumerable().SelectMany(x => x)));

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
			pathGenerator.Options.IgnoreDeprecatedCrs = IgnoreDeprecated;
			pathGenerator.Options.IgnoreDeprecatedOperations = IgnoreDeprecated;

			var transformGenerator = new StaticCoordinateOperationCompiler();

			int processedItems = 0;
			foreach (var batch in Batch(crsTestList, batchSize)) {
				Console.WriteLine(
					"{0:P6} ({1}/{2}) @{3}ops/s",
					(processedItems / (double)crsTestList.Count),
					processedItems,
					crsTestList.Count,
					processedItems / (stopwatch.ElapsedMilliseconds / 1000.0)
				);
				Parallel.ForEach(batch, testCase =>{
					try {
						testCase.Run(pathGenerator, transformGenerator);
					}
					catch (Exception ex) {
						Console.WriteLine("Failure on '{0}'.", testCase);
						Console.WriteLine(ex);
					}
				});
				processedItems += batch.Length;
				executedTestsBuffer.Add(batch);
			}

			stopwatch.Stop();
			Console.WriteLine("All tests complete");
			Console.WriteLine("Execution took: {0}", stopwatch.Elapsed);

			Console.Write("Completing report generation...");
			executedTestsBuffer.CompleteAdding();
			Task.WaitAll(writeResultsTask);
			Console.WriteLine("done!");
			
		}


	}
}
