using System;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgDatumTest : EpsgDataTestBase<EpsgDatum, DataTransmogrifier.EpsgDatum>
	{

		[Test]
		public void Resources_Match_Db() {

			var assemblyItems = EpsgMicroDatabase.Default.GetDatums();
			var dbItems = Repository.Datums;

			AssertMatches(
				assemblyItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Area.Code == y.AreaOfUse.Code),
				new Tester((x, y) => String.Equals(x.Type,y.Type, StringComparison.OrdinalIgnoreCase))
			);

		}

	}

	[TestFixture]
	public class EpsgDatumGeodeticTest : EpsgDataTestBase<EpsgDatumGeodetic, DataTransmogrifier.EpsgDatum>
	{

		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetGeodeticDatums();
			var dbItems = Repository.Datums.Where(x => String.Equals("Geodetic",x.Type,StringComparison.OrdinalIgnoreCase));

			AssertMatches(
				assemblyItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Area.Code == y.AreaOfUse.Code),
				new Tester((x, y) => String.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase)),
				new Tester((x, y) => x.PrimeMeridian.Code == y.PrimeMeridian.Code),
				new Tester((x, y) => x.Spheroid.Code == y.Ellipsoid.Code)
			);

		}

	}

}
