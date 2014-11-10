using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
    [TestFixture]
    public class AmericanPolyconicTest
    {

        [Test]
        public void gigs_sample_forward() {
            var degToRad = Math.PI / 180.0;
            var projection = new AmericanPolyconic(
                new GeographicCoordinate(0,-54.0 * degToRad),
                new Vector2(5000000, 10000000),
                new SpheroidEquatorialInvF(6378.137 * 1000, 298.2572221)
            );

            var input = new GeographicCoordinate(-6 * degToRad, -45 * degToRad);
            var expected = new Point2(5996378.71, 9328349.94);

            var result = projection.TransformValue(input);

            Assert.AreEqual(expected.X, result.X, 0.001);
            Assert.AreEqual(expected.Y, result.Y, 0.005);
        }

        [Test]
        public void gigs_sample_rev() {
            var degToRad = Math.PI / 180.0;
            var projection = new AmericanPolyconic(
                new GeographicCoordinate(0, -54.0 * degToRad),
                new Vector2(5000000, 10000000),
                new SpheroidEquatorialInvF(6378.137 * 1000, 298.2572221)
            );

            var expected = new GeographicCoordinate(-6 * degToRad, -45 * degToRad);
            var input = new Point2(5996378.71, 9328349.94);

            var result = projection.GetInverse().TransformValue(input);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.0006);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.00001);
        }

    }
}
