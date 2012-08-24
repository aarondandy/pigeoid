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

			var asmItems = EpsgCrs.Values;
			var dbItems = Repository.Crs;

			AssertMatches(
				asmItems,
				dbItems,
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

			var asmItems = EpsgCrsProjected.ProjectedValues.ToList();
			var dbItems = Repository.Crs
				//.Where(x => String.Equals(x.Kind,"projected",StringComparison.OrdinalIgnoreCase))
				.Where(x => String.Equals(x.Kind,"projected",StringComparison.OrdinalIgnoreCase)
					|| x.Projection != null
				)
				.OrderBy(x => x.Code)
				.ToList();

			AssertMatches(
				asmItems,
				dbItems,
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

			var asmItems = EpsgCrsCompound.CompoundValues.ToList();
			var dbItems = Repository.Crs
				.Where(x => String.Equals(x.Kind, "compound", StringComparison.OrdinalIgnoreCase))
				.OrderBy(x => x.Code)
				.ToList();

			AssertMatches(
				asmItems,
				dbItems,
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

		[Test]
		public void Resources_Match_Db() {

			var asmItems = EpsgCrsDatumBased.DatumBasedValues.ToList();
			var dbItems = Repository.Crs
				.Where(x =>
					!String.Equals(x.Kind, "compound", StringComparison.OrdinalIgnoreCase)
					&&
					!String.Equals(x.Kind, "projected", StringComparison.OrdinalIgnoreCase)
					&&
					x.Projection == null
				)
				.OrderBy(x => x.Code)
				.ToList();

			AssertMatches(
				asmItems,
				dbItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.Area.Code == y.Area.Code),
				new Tester((x, y) => x.Deprecated == y.Deprecated),
				new Tester((x, y) => x.CoordinateSystem.Code == y.CoordinateSystem.Code),
				new Tester((x, y) => x.Datum.Code == y.Datum.Code)

			);

		}

	}

}
