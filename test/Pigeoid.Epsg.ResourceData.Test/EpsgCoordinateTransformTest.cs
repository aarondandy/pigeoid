// TODO: source header

using System;
using System.Linq;
using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgCoordinateTransformTest : EpsgDataTestBase<EpsgCoordinateTransformInfo, DataTransmogrifier.EpsgCoordinateOperation>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgCoordinateOperationInfoRepository.TransformInfos;
			var dbItems = Repository.CoordinateOperations
				.Where(x => String.Equals("Transformation", x.TypeName, StringComparison.OrdinalIgnoreCase))
				.ToList();

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.OperationMethodInfo.Code == y.Method.Code),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.SourceCrsCode == y.SourceCrs.Code),
				new Tester((x, y) => x.TargetCrsCode == y.TargetCrs.Code),
				new Tester((x, y) => x.Accuracy == (y.Accuracy ?? 0))
			);

		}

	}
}
