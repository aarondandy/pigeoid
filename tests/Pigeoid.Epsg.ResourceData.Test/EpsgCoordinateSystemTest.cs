using System;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgCoordinateSystemTest : EpsgDataTestBase<EpsgCoordinateSystem, DataTransmogrifier.EpsgCoordinateSystem>
	{

		[Test]
		public void Resources_Match_Db() {

			var assemblyItems = EpsgMicroDatabase.Default.GetCoordinateSystems();
			var databaseItems = Repository.CoordinateSystems;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.Dimension == y.Dimension),
				new Tester((x, y) => String.Equals(x.Type.ToString(), y.TypeName,StringComparison.OrdinalIgnoreCase)),
				new Tester((x, y) => AreEqual(x.Axes.ToArray(), y.Axes.OrderBy(z => z.OrderValue).ToArray()))
			);

		}

		private static bool AreEqual(EpsgAxis[] x, DataTransmogrifier.EpsgAxis[] y) {
			Assert.AreEqual(x.Length, y.Length);
			for(int i = 0; i < x.Length; i++) {
				var a = x[i];
				var b = y[i];
				Assert.AreEqual(a.Name, b.Name);
				Assert.AreEqual(a.Abbreviation, b.Abbreviation);
				Assert.AreEqual(a.Orientation, b.Orientation);
				Assert.AreEqual(a.Unit.Code, b.Uom.Code);
			}
			return true;
		}
	}
}
