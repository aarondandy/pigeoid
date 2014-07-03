using DotSpatial.Projections;
using DotSpatial.Projections.ProjectedCategories;
using NUnit.Framework;
using Pigeoid.Epsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Interop.Proj4.Test
{
    [TestFixture]
    public class EpsgComparisonTests
    {

        public class TestSet
        {
            public TestSet(ProjectionInfo projectionInfo, EpsgCrs epsgCrs) {
                ProjectionInfo = projectionInfo;
                EpsgCrs = epsgCrs;
            }

            public ProjectionInfo ProjectionInfo { get; private set; }
            public EpsgCrs EpsgCrs { get; private set; }


            public override string ToString() {
                return "EPSG: " + EpsgCrs.Code + " proj4:" + ProjectionInfo.ToProj4String();
            }
        }

        public IEnumerable<TestSet> AllSets {
            get {

                var africaGeographic = new DotSpatial.Projections.GeographicCategories.Africa();
                var africaProjected = new DotSpatial.Projections.ProjectedCategories.Africa();
                var europeGeographic = new DotSpatial.Projections.GeographicCategories.Europe();
                var europeProjected = new DotSpatial.Projections.ProjectedCategories.Europe();

                yield return new TestSet(europeGeographic.NGO1948Oslo, EpsgCrs.Get(4817));
            }
        }

        [Test, TestCaseSource("AllSets")]
        public void Test(TestSet set) {

            var expectedProj4 = set.ProjectionInfo;
            var expectedEpsg = set.EpsgCrs;
            var actualProj4 = Pigeoid.Interop.Proj4.Proj4Crs.CreateProjection(expectedEpsg);
            var actualEpsg = Proj4Crs.Wrap(expectedProj4);

            Assert.AreEqual(expectedProj4.ToProj4String(), actualProj4.ToProj4String());

        }


    }
}
