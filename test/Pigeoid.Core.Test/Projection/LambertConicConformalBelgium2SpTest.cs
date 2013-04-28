using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
    [TestFixture]
    public class LambertConicConformalBelgium2SpTest
    {

        [Test]
        public void EpsgExample1314Test() {
            var projection = new LambertConicConformalBelgium(
                new GeographicCoordinate(1.57079633, 0.07604294),
                0.86975574,
                0.89302680,
                new Vector2(150000.01, 5400088.44),
                new SpheroidEquatorialInvF(6378388, 297)
            );

            var projected = projection.TransformValue(new GeographicCoordinate(0.88452540, 0.10135773));

            Assert.AreEqual(251763.20, projected.X, 0.008);
            Assert.AreEqual(153034.13, projected.Y, 0.03);
        }

        [Test]
        public void EpsgExample1314InverseTest() {
            var projection = new LambertConicConformalBelgium(
                new GeographicCoordinate(1.57079633, 0.07604294),
                0.86975574,
                0.89302680,
                new Vector2(150000.01, 5400088.44),
                new SpheroidEquatorialInvF(6378388, 297)
            );

            var unProjected = projection.GetInverse().TransformValue(new Point2(251763.20, 153034.13));

            Assert.AreEqual(0.88452540, unProjected.Latitude, 0.00000001);
            Assert.AreEqual(0.10135773, unProjected.Longitude, 0.000000002);
        }

    }
}
