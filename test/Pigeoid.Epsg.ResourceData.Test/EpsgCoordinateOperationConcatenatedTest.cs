// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgCoordinateOperationConcatenatedTest : EpsgDataTestBase<EpsgCoordinateOperationConcatenatedInfo, DataTransmogrifier.EpsgCoordinateOperation>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgCoordinateOperationInfoRepository.ConcatenatedInfos;
			var dbItems = Repository.CoordinateOperations
				.Where(x => String.Equals("Concatenated Operation", x.TypeName, StringComparison.OrdinalIgnoreCase))
				.ToList();

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.SourceCrs.Code == y.SourceCrs.Code),
				new Tester((x, y) => x.TargetCrs.Code == y.TargetCrs.Code),
				new Tester((x, y) => AreEqual(x.Steps.ToList(), y))
			);

		}

		private bool AreEqual(List<EpsgCoordinateOperationInfo> a, DataTransmogrifier.EpsgCoordinateOperation opB) {
			var b = Repository.CoordOpPathItems
				.Where(z => z.CatCode == opB.Code)
				.OrderBy(z => z.Step)
				.ToList();
			Assert.AreEqual(a.Count, b.Count);
			for (int i = 0; i < a.Count; i++) {
				var x = a[i];
				var y = b[i];
				Assert.AreEqual(y.Operation.Code, x.Code);
			}
			return true;
		}

	}
}
