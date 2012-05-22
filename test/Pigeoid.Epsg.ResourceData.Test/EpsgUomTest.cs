using System;
using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgUomTest : EpsgDataTestBase<EpsgUom, DataTransmogrifier.EpsgUom>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgUom.Values;
			var dbItems = Repository.Uoms;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.FactorB == (y.FactorB ?? 0)),
				new Tester((x, y) => x.FactorC == (y.FactorC ?? 0)),
				new Tester((x, y) => String.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase))
			);

		}


	}
}
