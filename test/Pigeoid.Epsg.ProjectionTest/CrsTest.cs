using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg.ProjectionTest
{
	public class CrsTest
	{

		public IEnumerable<Tuple<ICrs,ICrs>> GenerateAreaIntersectingCrs() {
			var items = EpsgCrs.Values.ToList();
			foreach (var fromCrs in items) {
				var fromArea = fromCrs.Area;
				foreach (var toCrs in items) {
					if (toCrs != fromCrs && fromArea.Intersects(toCrs.Area)) {
						yield return new Tuple<ICrs, ICrs>(fromCrs,toCrs);
					}
				}
			}
		}

		public void Run() {
			Console.Write("Generating test case count...");
			var crsTestList = GenerateAreaIntersectingCrs().ToList();
			Console.WriteLine("done.");

			Parallel.ForEach(crsTestList, testCrsSet => {
			//foreach (var testCrsSet in crsTestList) {
				var testCase = new CrsTestCase {From = testCrsSet.Item1, To = testCrsSet.Item2};
				testCase.Run();
			//}
			});

			throw new NotImplementedException();
		}


	}
}
