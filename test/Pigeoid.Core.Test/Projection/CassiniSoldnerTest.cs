using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
    [TestFixture]
    public class CassiniSoldnerTest
    {

        [Test]
        public void EpsgExample134Test() {
            var projection = new CassiniSoldner(
                new GeographicCoordinate(0.182241463, -1.070468608),
                new Vector2(430000.00, 325000.00),
                new SpheroidEquatorialInvF(31706587.88, 294.2606764)
            );

            var result = projection.TransformValue(new GeographicCoordinate(0.17453293, -1.08210414));
            var expected = new Point2(66644.94, 82536.22);

            Assert.AreEqual(expected.X, result.X, 0.2);
            Assert.AreEqual(expected.Y, result.Y, 0.2);
        }

        [Test]
        public void EpsgExample134InverseTest() {
            var projection = new CassiniSoldner(
                new GeographicCoordinate(0.182241463, -1.070468608),
                new Vector2(430000.00, 325000.00),
                new SpheroidEquatorialInvF(31706587.88, 294.2606764)
            );

            var result = projection.GetInverse().TransformValue(new Point2(66644.94, 82536.22));
            var expected = new GeographicCoordinate(0.17453293, -1.08210414);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.00000004);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.000000004);
        }

    }
}
