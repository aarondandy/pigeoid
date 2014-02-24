using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotSpatial.Projections;
using DotSpatial.Projections.ProjectedCategories;
using NUnit.Framework;
using Pigeoid.Epsg;
using Pigeoid.Ogc;

namespace Pigeoid.Interop.Proj4.Test
{
    [TestFixture]
    public class ProjectionInfoConversionTests
    {

        private TransverseMercatorSystems TransverseMercatorSystems = new TransverseMercatorSystems();

        [Test]
        public void transversemercator_WGS1984lo33() {
            var expected = TransverseMercatorSystems.WGS1984lo33;
            var actual = Proj4CrsProjected.CreateProjection(new Proj4CrsProjected(TransverseMercatorSystems.WGS1984lo33));
            Assert.AreEqual(expected.ToProj4String(), actual.ToProj4String());
            Assert.Inconclusive();
        }

    }
}
