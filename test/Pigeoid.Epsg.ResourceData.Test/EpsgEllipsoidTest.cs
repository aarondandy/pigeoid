
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgEllipsoidTest : EpsgDataTestBase<EpsgEllipsoid, DataTransmogrifier.EpsgEllipsoid>
	{

		[Test]
		public void Resources_Match_Db() {

			var assemblyItems = EpsgEllipsoid.Values;
			var databaseItems = Repository.Ellipsoids;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.AxisUnit.Code == y.Uom.Code),
// ReSharper disable CompareOfFloatsByEqualityOperator
				new Tester((x, y) => x.A == y.SemiMajorAxis),
				new Tester((x, y) => 
					(y.SemiMinorAxis.HasValue && x.B == y.SemiMinorAxis.Value)
					|| (y.InverseFlattening.HasValue && x.InvF == y.InverseFlattening.Value)
				)
// ReSharper restore CompareOfFloatsByEqualityOperator
			);

		}


	}
}
