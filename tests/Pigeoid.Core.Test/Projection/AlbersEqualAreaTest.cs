using NUnit.Framework;
using System;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
    [TestFixture]
    public class AlbersEqualAreaTest
    {

        [Test]
        public void gigs_sample_test() {
            var projection = new AlbersEqualArea(
                new GeographicCoordinate(0, 132 * Math.PI / 180.0),
                -18 * Math.PI / 180.0,
                -36 * Math.PI / 180.0,
                new Vector2(0,0),
                new SpheroidEquatorialInvF(6378.137 * 1000, 298.2572221));
            Assert.That(projection.HasInverse);
            var inverse = projection.GetInverse();

            var expectedGeographic = new GeographicCoordinate(-40 * Math.PI / 180.0, 140 * Math.PI / 180.0);
            var expectedProjected = new Point2(693250.21, -4395794.49);

            var actualProjected = projection.TransformValue(expectedGeographic);
            Assert.AreEqual(expectedProjected.X, actualProjected.X, 0.0008);
            Assert.AreEqual(expectedProjected.Y, actualProjected.Y, 0.006);

            var actualGeographic = inverse.TransformValue(expectedProjected);
            Assert.AreEqual(expectedGeographic.Latitude, actualGeographic.Latitude, 0.000000001);
            Assert.AreEqual(expectedGeographic.Longitude, actualGeographic.Longitude, 0.0000000003);
        }

    }
}
