
using System;
using System.Diagnostics;

namespace Pigeoid.Epsg.ProjectionTest
{
	class Program
	{
		static void Main(string[] args) {
			//for (int i = 0; i < 10; i++)
			//	Test();

			//Console.ReadKey();
			var test = new CrsTest();
			test.Run();
		}

		static void Test() {
			var generator = new EpsgCrsCoordinateOperationPathGenerator();
			var from = EpsgCrs.Get(4267); //EpsgCrs.Get(26701);
			var to = EpsgCrs.Get(4025);

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var path = generator.Generate(from, to);
			stopwatch.Stop();
			Console.WriteLine("{0} ({1})", stopwatch.Elapsed, stopwatch.Elapsed.TotalMilliseconds);
		}

	}
}
