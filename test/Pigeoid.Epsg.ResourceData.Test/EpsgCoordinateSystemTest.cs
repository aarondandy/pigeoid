using System;
using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgCoordinateSystemTest : EpsgDataTestBase<EpsgCoordinateSystem, DataTransmogrifier.EpsgCoordinateSystem>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgCoordinateSystem.Values;
			var dbItems = Repository.CoordinateSystems;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.Dimension == y.Dimension),
				new Tester((x, y) => String.Equals(x.Type.ToString(), y.TypeName,StringComparison.OrdinalIgnoreCase))
			);

		}


	}
}
