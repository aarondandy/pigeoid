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

        [Test]
        public void epsg26931_to_epsg3574() {
            var from = EpsgCrs.Get(26931);
            Assert.IsNotNull(from);
            var to = EpsgCrs.Get(3574);
            Assert.IsNotNull(to);

            var prjFrom = Proj4Crs.CreateProjection(from);
            Assert.IsNotNull(prjFrom);
            Assert.AreEqual(57, prjFrom.LatitudeOfOrigin);
            Assert.AreEqual(-133.6666666666667, prjFrom.LongitudeOfCenter, 0.000000001);
            Assert.AreEqual(323.1301023611111, prjFrom.alpha, 0.000000001);
            Assert.AreEqual(0.9999, prjFrom.ScaleFactor, 0.000000001);
            Assert.AreEqual(5000000, prjFrom.FalseEasting, 0.000000001);
            Assert.AreEqual(-5000000, prjFrom.FalseNorthing, 0.000000001);

            var prjTo = Proj4Crs.CreateProjection(to);
            Assert.IsNotNull(prjTo);
            Assert.AreEqual(90, prjTo.LatitudeOfOrigin);
            Assert.AreEqual(-40, prjTo.CentralMeridian);
            Assert.AreEqual(0, prjTo.FalseEasting);
            Assert.AreEqual(0, prjTo.FalseNorthing);
        }

        [Test]
        public void epsg2281_to_epsg32411() {
            var from = EpsgCrs.Get(2281);
            Assert.IsNotNull(from);
            var to = EpsgCrs.Get(32411);
            Assert.IsNotNull(to);

            var prjFrom = Proj4Crs.CreateProjection(from);
            Assert.IsNotNull(prjFrom);
            Assert.AreEqual(40.65, prjFrom.StandardParallel1);
            Assert.AreEqual(39.01666666666667, prjFrom.StandardParallel2, 0.0000000001);
            Assert.AreEqual(-111.5, prjFrom.CentralMeridian);
            Assert.AreEqual(500000, prjFrom.FalseEasting, 0.001);
            Assert.AreEqual(2000000, prjFrom.FalseNorthing, 0.001);

            var prjTo = Proj4Crs.CreateProjection(to);
            Assert.IsNotNull(prjTo);
        }


    }
}
