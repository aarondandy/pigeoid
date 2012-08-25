using NUnit.Framework;
using Vertesaur;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgAreaTest : EpsgDataTestBase<EpsgArea,DataTransmogrifier.EpsgArea>
	{

		[Test]
		public void Resources_Match_Db() {

			var assemblyItems = EpsgArea.Values;
			var databaseItems = Repository.Areas;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Iso2 == y.Iso2),
				new Tester((x, y) => x.Iso3 == y.Iso3),
				new Tester((x, y) => x.LatitudeRange == new Range(y.NorthBound ?? 0,y.SouthBound ?? 0)),
// ReSharper disable CompareOfFloatsByEqualityOperator
				new Tester((x, y) => x.LongitudeRange.Start == (y.WestBound ?? 0)),
				new Tester((x, y) => x.LongitudeRange.End == (y.EastBound ?? 0))
// ReSharper restore CompareOfFloatsByEqualityOperator
			);

		}

	}
}
