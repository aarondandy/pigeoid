using NUnit.Framework;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;

namespace Pigeoid.Core.Test.Transformation
{
    [TestFixture]
    public class GeocentricTopocentricTransformationTest
    {

        [Test]
        public void EpsgExample222Test() {
            var geocentric = new Point3(3771793.968, 140253.342, 5124304.349);
            var topocentric = new Point3(-189013.869, -128642.040, -4220.171);
            var transform = new GeocentricTopocentricTransformation(
                new Point3(3652755.3058, 319574.6799, 5201547.3536),
                new SpheroidEquatorialInvF(6378137.0, 298.2572236)
            );

            var result = transform.TransformValue(geocentric);

            Assert.AreEqual(topocentric.X, result.X, 0.0001);
            Assert.AreEqual(topocentric.Y, result.Y, 0.0004);
            Assert.AreEqual(topocentric.Z, result.Z, 0.0002);

        }

        [Test]
        public void EpsgExample222InverseTest() {
            var geocentric = new Point3(3771793.968, 140253.342, 5124304.349);
            var topocentric = new Point3(-189013.869, -128642.040, -4220.171);
            var transform = new GeocentricTopocentricTransformation(
                new Point3(3652755.3058, 319574.6799, 5201547.3536),
                new SpheroidEquatorialInvF(6378137.0, 298.2572236)
            );

            var result = transform.GetInverse().TransformValue(topocentric);

            Assert.AreEqual(geocentric.X, result.X, 0.0004);
            Assert.AreEqual(geocentric.Y, result.Y, 0.00006);
            Assert.AreEqual(geocentric.Z, result.Z, 0.00003);
        }

    }
}
