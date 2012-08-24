
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgPrimeMeridianTest : EpsgDataTestBase<EpsgPrimeMeridian, DataTransmogrifier.EpsgPrimeMeridian>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgPrimeMeridian.Values;
			var dbItems = Repository.PrimeMeridians;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Unit.Code == y.Uom.Code),
				new Tester((x, y) => x.Longitude == y.GreenwichLon)
			);

		}


	}
}
