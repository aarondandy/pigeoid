using System;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgUomTest : EpsgDataTestBase<EpsgUom, DataTransmogrifier.EpsgUom>
	{

		[Test]
		public void Resources_Match_Db() {

			var assemblyItems = EpsgUom.Values;
			var databaseItems = Repository.Uoms;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
// ReSharper disable CompareOfFloatsByEqualityOperator
				new Tester((x, y) => x.FactorB == (y.FactorB ?? 0)),
				new Tester((x, y) => x.FactorC == (y.FactorC ?? 0)),
// ReSharper restore CompareOfFloatsByEqualityOperator
				new Tester((x, y) => String.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase))
			);

		}


	}
}
