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
            var crs26931 = EpsgCrs.Get(26931);
            Assert.IsNotNull(crs26931);
            var crs3574 = EpsgCrs.Get(3574);
            Assert.IsNotNull(crs3574);

            var prj26931 = Proj4Crs.CreateProjection(crs26931);
            Assert.IsNotNull(prj26931);
            Assert.AreEqual(57, prj26931.LatitudeOfOrigin);
            Assert.AreEqual(-133.6666666666667, prj26931.LongitudeOfCenter, 0.000000001);
            Assert.AreEqual(323.1301023611111, prj26931.alpha, 0.000000001);
            Assert.AreEqual(0.9999, prj26931.ScaleFactor, 0.000000001);
            Assert.AreEqual(5000000, prj26931.FalseEasting, 0.000000001);
            Assert.AreEqual(-5000000, prj26931.FalseNorthing, 0.000000001);

            var prj3574 = Proj4Crs.CreateProjection(crs3574);
            Assert.IsNotNull(prj3574);
            Assert.AreEqual(90, prj3574.LatitudeOfOrigin);
            Assert.AreEqual(-40, prj3574.CentralMeridian);
            Assert.AreEqual(0, prj3574.FalseEasting);
            Assert.AreEqual(0, prj3574.FalseNorthing);
        }


    }
}
