using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgEllipsoidTest : EpsgDataTestBase<EpsgEllipsoid, DataTransmogrifier.EpsgEllipsoid>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgEllipsoid.Values;
			var dbItems = Repository.Ellipsoids;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Unit.Code == y.Uom.Code),
				new Tester((x, y) => x.A == y.SemiMajorAxis),
				new Tester((x, y) => 
					(y.SemiMinorAxis.HasValue && x.B == y.SemiMinorAxis.Value)
					|| (y.InverseFlattening.HasValue && x.InvF == y.InverseFlattening.Value)
				)
			);

		}


	}
}
