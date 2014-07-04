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
        public void epsg26931() {
            var crs = EpsgCrs.Get(26931);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(57, prj.LatitudeOfOrigin);
            Assert.AreEqual(-133.6666666666667, prj.LongitudeOfCenter, 0.000000001);
            Assert.AreEqual(323.1301023611111, prj.alpha, 0.000000001);
            Assert.AreEqual(0.9999, prj.ScaleFactor, 0.000000001);
            Assert.AreEqual(5000000, prj.FalseEasting, 0.000000001);
            Assert.AreEqual(-5000000, prj.FalseNorthing, 0.000000001);
        }

        [Test]
        public void epsg3574() {
            var crs = EpsgCrs.Get(3574);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(90, prj.LatitudeOfOrigin);
            Assert.AreEqual(-40, prj.CentralMeridian);
            Assert.AreEqual(0, prj.FalseEasting);
            Assert.AreEqual(0, prj.FalseNorthing);
        }

        [Test]
        public void epsg2281() {
            var crs = EpsgCrs.Get(2281);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(40.65, prj.StandardParallel1);
            Assert.AreEqual(39.01666666666667, prj.StandardParallel2, 0.0000000001);
            Assert.AreEqual(-111.5, prj.CentralMeridian);
            Assert.AreEqual(500000, prj.FalseEasting, 0.001);
            Assert.AreEqual(2000000, prj.FalseNorthing, 0.001);
        }

        [Test]
        public void epsg3079() {
            var crs = EpsgCrs.Get(3079);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(45.30916666666666, prj.LatitudeOfOrigin, 0.00000001);
            Assert.AreEqual(-86, prj.LongitudeOfCenter);
            Assert.AreEqual(0.9996, prj.ScaleFactor);
            Assert.AreEqual(2546731.496, prj.FalseEasting);
            Assert.AreEqual(-4354009.816, prj.FalseNorthing);
        }


    }
}
