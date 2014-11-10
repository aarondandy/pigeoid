using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
    [TestFixture]
    public class EquidistantCylindricalTest
    {

        [Test]
        public void epsg_example_1_3_14() {
            var projection = new EquidistantCylindrical(GeographicCoordinate.Zero, Vector2.Zero, new SpheroidEquatorialInvF(6378137, 298.257223563));

            var input = new GeographicCoordinate(0.959931086, 0.174532925);
            var expected = new Point2(1113194.91, 6097230.31);

            var result = projection.TransformValue(input);

            Assert.AreEqual(expected.X, result.X, 0.004);
            Assert.AreEqual(expected.Y, result.Y, 0.02);
        }

        [Test]
        public void epsg_example_1_3_14_inverse() {
            var projection = new EquidistantCylindrical(GeographicCoordinate.Zero, Vector2.Zero, new SpheroidEquatorialInvF(6378137, 298.257223563))
                .GetInverse();

            var input = new Point2(1113194.91, 6097230.31);
            var expected = new GeographicCoordinate(0.959931086, 0.174532925);

            var result = projection.TransformValue(input);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000003);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.0000000006);
        }

    }
}
