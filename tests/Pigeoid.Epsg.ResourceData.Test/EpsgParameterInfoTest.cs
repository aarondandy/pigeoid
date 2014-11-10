
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
    public class EpsgParameterInfoTest : EpsgDataTestBase<EpsgParameterInfo, DbRepository.EpsgParameter>
	{

		[Test]
		public void Resources_Match_Db() {

			var assemblyItems = EpsgMicroDatabase.Default.GetParameterInfos();
			var databaseItems = Repository.Parameters;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name)
			);

		}


	}
}
