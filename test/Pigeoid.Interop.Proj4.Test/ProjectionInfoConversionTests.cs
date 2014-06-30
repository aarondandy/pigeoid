using DotSpatial.Projections.ProjectedCategories;
using NUnit.Framework;

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
        }

    }
}
