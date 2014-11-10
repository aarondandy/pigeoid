
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
    public class EpsgPrimeMeridianTest : EpsgDataTestBase<EpsgPrimeMeridian, DbRepository.EpsgPrimeMeridian>
	{

		[Test]
		public void Resources_Match_Db() {

			var assemblyItems = EpsgMicroDatabase.Default.GetPrimeMeridians();
			var databaseItems = Repository.PrimeMeridians;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Unit.Code == y.Uom.Code),
// ReSharper disable CompareOfFloatsByEqualityOperator
				new Tester((x, y) => x.Longitude == y.GreenwichLon)
// ReSharper restore CompareOfFloatsByEqualityOperator
			);

		}


	}
}
