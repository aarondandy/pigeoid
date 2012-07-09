using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.ProjectionTest
{
	class Program
	{
		static void Main(string[] args) {

			var txGen = new EpsgTransformationGenerator();
			var from = EpsgCrs.Get(4326);
			var to = EpsgCrs.Get(3857);
			var tx = txGen.Generate(from, to);
			;
		}
	}
}
