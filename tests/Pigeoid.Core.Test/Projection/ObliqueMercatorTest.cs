using System;
using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
    [TestFixture]
    public class ObliqueMercatorTest
    {

        [Test]
        public void Epsg_1_3_6_1_Test() {
            var projection = new HotineObliqueMercator.VariantB(
                new GeographicCoordinate(0.069813170, 2.007128640),
                0.930536611,
                0.927295218,
                0.99984,
                new Vector2(590476.87, 442857.65),
                new SpheroidEquatorialInvF(6377298.556, 300.8017)
            );
            var input = new GeographicCoordinate(0.094025313, 2.021187362);
            var expected = new Point2(679245.73, 596562.78);

            var result = projection.TransformValue(input);

            Assert.AreEqual(expected.X, result.X, 0.003);
            Assert.AreEqual(expected.Y, result.Y, 0.001);
        }

        [Test]
        public void Epsg_1_3_6_1_Inverse_Test() {
            var projectionForward = new HotineObliqueMercator.VariantB(
                new GeographicCoordinate(0.069813170, 2.007128640),
                0.930536611,
                0.927295218,
                0.99984,
                new Vector2(590476.87, 442857.65),
                new SpheroidEquatorialInvF(6377298.556, 300.8017)
            );
            Assert.That(projectionForward.HasInverse);
            var inverse = projectionForward.GetInverse();

            var expected = new GeographicCoordinate(0.094025313, 2.021187362);
            var input = new Point2(679245.73, 596562.78);

            var result = inverse.TransformValue(input);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.00001);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.0000000005);
        }

        [Test]
        public void Epsg_1_3_6_2_Test() {
            var projection = new LabordeObliqueMercator(
                new GeographicCoordinate(-0.329867229, 0.810482544 - (2.5969212963 * Math.PI / 200.0)),
                0.329867229,
                0.9995,
                new SpheroidEquatorialInvF(6378388, 297),
                new Vector2(400000, 800000)
            );

            var lonFromParis = 0.735138668;
            var lonFromGreenwich = lonFromParis + (2.5969212963 * Math.PI / 200.0);
            var input = new GeographicCoordinate(-0.282565315, lonFromGreenwich);
            var expected = new Point2(188333.848, 1098841.091);

            var result = projection.TransformValue(input);

            Assert.AreEqual(expected.X, result.X, 50);
            Assert.AreEqual(expected.Y, result.Y, 50);
        }

        [Test]
        public void Epsg_1_3_6_2_Inverse_Test() {
            var projection = new LabordeObliqueMercator(
                new GeographicCoordinate(-0.329867229, 0.810482544 - (2.5969212963 * Math.PI / 200.0)),
                0.329867229,
                0.9995,
                new SpheroidEquatorialInvF(6378388, 297),
                new Vector2(400000, 800000)
            );
            Assert.That(projection.HasInverse);
            var unProjection = projection.GetInverse();

            var lonFromParis = 0.735138668;
            var lonFromGreenwich = lonFromParis + (2.5969212963 * Math.PI / 200.0);
            var expected = new GeographicCoordinate(-0.282565315, lonFromGreenwich);
            var input = new Point2(188333.848, 1098841.091);

            var result = unProjection.TransformValue(input);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.00001);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.00002);
        }

    }
}
