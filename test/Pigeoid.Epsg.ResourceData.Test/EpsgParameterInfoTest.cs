
using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgParameterInfoTest : EpsgDataTestBase<EpsgParameterInfo, DataTransmogrifier.EpsgParameter>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgParameterInfo.Values;
			var dbItems = Repository.Parameters;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name)
			);

		}


	}
}
