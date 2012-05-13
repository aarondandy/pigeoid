using System;
using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgAreaTest : EpsgDataTestBase<EpsgArea,DataTransmogrifier.EpsgArea>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgArea.Values;
			var dbItems = Repository.Areas;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x,y) => x.Code == y.Code),
				new Tester((x,y) => x.Name == y.Name),
				new Tester((x,y) => x.Iso2 == y.Iso2),
				new Tester((x,y) => x.Iso3 == y.Iso3)
			);

		}


	}
}
