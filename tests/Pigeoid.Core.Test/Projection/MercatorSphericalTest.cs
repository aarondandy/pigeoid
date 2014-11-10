using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
    [TestFixture]
    public class MercatorSphericalTest
    {

        [Test]
        public void EpsgExample1331Test() {
            var projection = new MercatorSpherical(6371007.0);

            var projected = projection.TransformValue(new GeographicCoordinate(0.425542460, -1.751147016));

            Assert.AreEqual(-11156569.90, projected.X, 0.01);
            Assert.AreEqual(2796869.94, projected.Y, 0.003);
        }

        [Test]
        public void EpsgExample1331InverseTest() {
            var projection = new MercatorSpherical(6371007.0);

            var unProjected = projection.GetInverse().TransformValue(new Point2(-11156569.90, 2796869.94));

            Assert.AreEqual(0.425542460, unProjected.Latitude, 0.000000001);
            Assert.AreEqual(-1.751147016, unProjected.Longitude, 0.0000000005);
        }

        [Test]
        public void EpsgExample1332Test() {
            var projection = new MercatorSpherical(6378137.0);

            var unProjected = projection.GetInverse().TransformValue(new Point2(-11169055.58, 2800000.00));

            Assert.AreEqual(0.425542460, unProjected.Latitude, 0.000000001);
            Assert.AreEqual(-1.751147016, unProjected.Longitude, 0.0000000008);
        }

        [Test]
        public void EpsgExample1332InverseTest() {
            var projection = new MercatorSpherical(6378137.0);

            var unProjected = projection.GetInverse().TransformValue(new Point2(-11169055.58, 2800000.00));

            Assert.AreEqual(0.425542460, unProjected.Latitude, 0.000000001);
            Assert.AreEqual(-1.751147016, unProjected.Longitude, 0.0000000008);
        }

    }
}
