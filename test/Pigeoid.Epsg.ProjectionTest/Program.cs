
using System;
using System.Diagnostics;

namespace Pigeoid.Epsg.ProjectionTest
{
	class Program
	{
		static void Main(string[] args) {
			//Test();

			//for (int i = 0; i < 10; i++)
			//	Test();
			
			var test = new CrsTest();
			test.Run();

			Console.WriteLine("DONE");
			Console.ReadKey();
		}

		static void Test() {
			var generator = new EpsgCrsCoordinateOperationPathGenerator();
			var from = EpsgCrs.Get(26701); // EpsgCrs.Get(4267);
			var to = EpsgCrs.Get(4401);

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var path = generator.Generate(from, to);
			stopwatch.Stop();
			Console.WriteLine("{0} ({1})", stopwatch.Elapsed, stopwatch.Elapsed.TotalMilliseconds);
		}

	}
}
