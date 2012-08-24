using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgCoordOpMethodInfoTest : EpsgDataTestBase<EpsgCoordOpMethodInfo, DataTransmogrifier.EpsgCoordinateOperationMethod>
	{

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgCoordOpMethodInfo.Values;
			var dbItems = Repository.CoordinateOperationMethods;

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.CanReverse == y.Reverse),
				new Tester((x, y) => AssertEqual(x.ParameterUsage,y.ParamUse.OrderBy(z => z.SortOrder).ToList()))
			);

		}

		private bool AssertEqual(ReadOnlyCollection<EpsgCoordOpMethodInfo.ParamUsage> a, IList<DataTransmogrifier.EpsgParamUse> b) {
			Assert.AreEqual(a.Count, b.Count);
			for(int i = 0; i < a.Count; i++) {
				var x = a[i];
				var y = b[i];
				Assert.AreEqual(x.Parameter.Code, y.Parameter.Code);
				Assert.AreEqual(x.SignReversal, y.SignReversal ?? false);
			}
			return true;
		}

		[Test]
		public void Operation_Param_Values() {
			foreach(var dbOpMethod in Repository.CoordinateOperationMethods) {
				var asmOpMethod = EpsgCoordOpMethodInfo.Get(dbOpMethod.Code);
				Assert.IsNotNull(asmOpMethod);
				foreach(var dbOperation in dbOpMethod.UsedBy) {
					var asmParamValues = asmOpMethod.GetOperationParameters(dbOperation.Code);
					var dbParamValues = dbOperation.ParameterValues.Where(x => x.NumericValue != null || x.TextValue != null).ToList();
					Assert.AreEqual(dbParamValues.Count, asmParamValues.Count);
					foreach(var dbParamValue in dbParamValues) {
						var asmParamValue = asmParamValues.First(x => x.Name == dbParamValue.Parameter.Name);
						var dbValue = (object)dbParamValue.NumericValue ?? dbParamValue.TextValue;
						var dbUom = dbParamValue.Uom;
						Assert.AreEqual(dbValue, asmParamValue.Value);
						if(null == dbUom)
							Assert.IsNull(asmParamValue.Unit);
						else
							Assert.AreEqual(dbUom.Code, (asmParamValue.Unit as EpsgUom).Code);
					}
				}
			}
		}
	}
}
