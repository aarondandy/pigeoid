// TODO: source header

using System;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgCoordinateTransformTest : EpsgDataTestBase<EpsgCoordinateTransformInfo, DataTransmogrifier.EpsgCoordinateOperation>
	{

		[Test]
		public void Resources_Match_Db() {

			var assemblyItems = EpsgCoordinateOperationInfoRepository.TransformInfos;
			var databaseItems = Repository.CoordinateOperations
				.Where(x => String.Equals("Transformation", x.TypeName, StringComparison.OrdinalIgnoreCase))
				.ToList();

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.OperationMethodInfo.Code == y.Method.Code),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.SourceCrs.Code == y.SourceCrs.Code),
				new Tester((x, y) => x.TargetCrs.Code == y.TargetCrs.Code),
// ReSharper disable CompareOfFloatsByEqualityOperator
				new Tester((x, y) => x.Accuracy == (y.Accuracy ?? 0))
// ReSharper restore CompareOfFloatsByEqualityOperator
			);

		}

	}
}
