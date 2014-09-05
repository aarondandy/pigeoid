using System;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
	public class EpsgCrsTest : EpsgDataTestBase<EpsgCrs, DataTransmogrifier.EpsgCrs>
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
	public class EpsgCrsProjectedTest : EpsgDataTestBase<EpsgCrsProjected, DataTransmogrifier.EpsgCrs>
	{

		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetAllCrs().OfType<EpsgCrsProjected>().ToList();
			var databaseItems = Repository.Crs
				.Where(x => String.Equals(x.Kind,"projected",StringComparison.OrdinalIgnoreCase))
				.OrderBy(x => x.Code)
				.ToList();

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
	public class EpsgCrsCompoundTest : EpsgDataTestBase<EpsgCrsCompound, DataTransmogrifier.EpsgCrs>
	{

		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetAllCrs().OfType<EpsgCrsCompound>().ToList();
			var databaseItems = Repository.Crs
				.Where(x => String.Equals(x.Kind, "compound", StringComparison.OrdinalIgnoreCase))
				.OrderBy(x => x.Code)
				.ToList();

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
	public class EpsgCrsDatumBasedTest : EpsgDataTestBase<EpsgCrsDatumBased, DataTransmogrifier.EpsgCrs>
	{

        private static bool CompareDatums(EpsgCrsDatumBased x, DataTransmogrifier.EpsgCrs y) {
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
			var databaseItems = Repository.Crs
				.Where(x =>
					!String.Equals(x.Kind, "compound", StringComparison.OrdinalIgnoreCase)
				)
				.OrderBy(x => x.Code)
				.ToList();

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
