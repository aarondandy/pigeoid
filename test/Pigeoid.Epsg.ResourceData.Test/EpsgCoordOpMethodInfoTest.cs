using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgCoordOpMethodInfoTest : EpsgDataTestBase<EpsgCoordOpMethodInfo, DataTransmogrifier.EpsgCoordinateOperationMethod>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgCoordOpMethodInfo.Values;
			var dbItems = Repository.CoordinateOperationMethods;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.CanReverse == y.Reverse)
			);

		}
	}
}
