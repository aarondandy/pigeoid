using System;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
    public class EpsgCrsTest : EpsgDataTestBase<EpsgCrs, DbRepository.EpsgCrs>
	{

		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetAllCrs();
			var databaseItems = Repository.Crs;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated)
			);

		}

	}

	[TestFixture]
    public class EpsgCrsProjectedTest : EpsgDataTestBase<EpsgCrsProjected, DbRepository.EpsgCrs>
	{

		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetAllCrs().OfType<EpsgCrsProjected>().ToList();
			var databaseItems = Repository.CrsProjected;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.BaseCrs.Code == y.SourceGeographicCrs.Code),
				new Tester((x, y) => x.CoordinateSystem.Code == y.CoordinateSystem.Code)
				// TODO: test the projection op
			);

		}

	}

	[TestFixture]
    public class EpsgCrsCompoundTest : EpsgDataTestBase<EpsgCrsCompound, DbRepository.EpsgCrs>
	{

		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetAllCrs().OfType<EpsgCrsCompound>().ToList();
			var databaseItems = Repository.CrsCompound;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.Horizontal.Code == y.CompoundHorizontalCrs.Code),
				new Tester((x, y) => x.Vertical.Code == y.CompoundVerticalCrs.Code)
			);

		}

	}

	[TestFixture]
    public class EpsgCrsDatumBasedTest : EpsgDataTestBase<EpsgCrsDatumBased, DbRepository.EpsgCrs>
	{

        private static bool CompareDatums(EpsgCrsDatumBased x, DbRepository.EpsgCrs y) {
            var testDatum = y.Datum;
            if (testDatum == null) {
                var searchCrs = y;
                while (searchCrs != null) {
                    testDatum = searchCrs.Datum;
                    if (testDatum != null)
                        break;
                    searchCrs = searchCrs.SourceGeographicCrs;
                }
            }
            return x.Datum.Code == testDatum.Code;
        }


		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetAllCrs().OfType<EpsgCrsDatumBased>().ToList();
			var databaseItems = Repository.CrsNotCompound;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.CoordinateSystem.Code == y.CoordinateSystem.Code),
                new Tester((x, y) => CompareDatums(x,y))
			);

		}

	}

}
