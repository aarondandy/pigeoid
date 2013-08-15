using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{

    [TestFixture]
    public class LambertAzimuthalEqualAreaTest
    {

        /// <summary>
        /// Lambert Azimuthal Equal Area
        /// </summary>
        [Test]
        public void epsg_example_1_3_11() {
            var projection = new LambertAzimuthalEqualArea(
                new GeographicCoordinate(0.907571211, 0.174532925),
                new Vector2(4321000, 3210000),
                new SpheroidEquatorialInvF(6378137, 298.2572221));

            var inverse = projection.GetInverse();
            Assert.IsNotNull(inverse);

            var projectedExpected = new Point2(3962799.45, 2999718.85);
            var geographicExpected = new GeographicCoordinate(0.872664626, 0.087266463);

            var projectedActual = projection.TransformValue(geographicExpected);
            Assert.AreEqual(projectedExpected.X, projectedActual.X, 0.004);
            Assert.AreEqual(projectedExpected.Y, projectedActual.Y, 0.004);

            var geographicActual = inverse.TransformValue(projectedExpected);
            Assert.AreEqual(geographicExpected.Latitude, geographicActual.Latitude, 0.000001);
            Assert.AreEqual(geographicExpected.Longitude, geographicActual.Longitude, 0.00000003);
        }

    }
}
