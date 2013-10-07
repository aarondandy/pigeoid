using System;

namespace Pigeoid.Epsg.ProjectionTest
{
    class Program
    {
        static void Main(string[] args) {
            var test = new CrsTest();
            test.Run();
            Console.WriteLine("Press the [Any] key to close.");
            Console.ReadKey();
        }

    }
}
