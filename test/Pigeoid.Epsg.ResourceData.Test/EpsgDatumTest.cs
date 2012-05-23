
using System;
using System.Linq;
using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgDatumTest : EpsgDataTestBase<EpsgDatum, DataTransmogrifier.EpsgDatum>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgDatum.Values;
			var dbItems = Repository.Datums;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => String.Equals(x.Type,y.Type, StringComparison.OrdinalIgnoreCase))
			);

		}

	}

	[TestFixture]
	public class EpsgDatumGeodeticTest : EpsgDataTestBase<EpsgDatumGeodetic, DataTransmogrifier.EpsgDatum>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgDatum.Values.OfType<EpsgDatumGeodetic>();
			var dbItems = Repository.Datums.Where(x => String.Equals("Geodetic",x.Type,StringComparison.OrdinalIgnoreCase));

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => String.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase)),
				new Tester((x, y) => x.PrimeMeridian.Code == y.PrimeMeridian.Code),
				new Tester((x, y) => x.Spheroid.Code == y.Ellipsoid.Code)
			);

		}

	}

}
