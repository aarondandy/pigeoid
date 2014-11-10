// TODO: source header

using System;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
    public class EpsgCoordinateConversionTest : EpsgDataTestBase<EpsgCoordinateOperationInfo, DbRepository.EpsgCoordinateOperation>
	{

		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetCoordinateConversionInfos();
            var databaseItems = Repository.CoordinateConversions;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Method.Code == y.Method.Code),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated)
			);

		}

	}
}
