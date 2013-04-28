using System;
using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
    [TestFixture]
    public class TransverseMercatorTest
    {

        [Test]
        public void EpsgExample1351Test() {
            var projection = new TransverseMercator(
                new GeographicCoordinate(0.85521133, -0.03490659),
                new Vector2(400000.00, -100000.00),
                0.9996012717,
                new SpheroidEquatorialInvF(6377563.396, 299.32496)
            );

            var input = new GeographicCoordinate(0.88139127, 0.00872665);
            var expected = new Point2(577274.99, 69740.50);

            var result = projection.TransformValue(input);

            Assert.AreEqual(expected.X, result.X, 0.03);
            Assert.AreEqual(expected.Y, result.Y, 0.002);
        }

        [Test]
        public void EpsgExample1351InverseTest() {
            var projection = new TransverseMercator(
                new GeographicCoordinate(0.85521133, -0.03490659),
                new Vector2(400000.00, -100000.00),
                0.9996012717,
                new SpheroidEquatorialInvF(6377563.396, 299.32496)
            );

            var input = new Point2(577274.99, 69740.50);
            var expected = new GeographicCoordinate(0.88139127, 0.00872665);

            var result = projection.GetInverse().TransformValue(input);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.00000000004);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.000000008);
        }

        [Test]
        public void EpsgExample_1_3_5_3_Test() {
            var projection = new TransverseMercatorSouth(
                new GeographicCoordinate(0, 0.506145483),
                new Vector2(0, 0),
                1,
                new SpheroidEquatorialInvF(6378137, 298.25722356)
            );
            var expected = new Point2(2847342.74, 71984.49);
            var input = new GeographicCoordinate(-0.449108618, 0.493625066);

            var result = projection.TransformValue(input);

            Assert.AreEqual(expected.X, result.X, 0.003);
            Assert.AreEqual(expected.Y, result.Y, 0.002);
        }

        [Test]
        public void EpsgExample_1_3_5_3_InverseTest() {
            var projection = new TransverseMercatorSouth(
                new GeographicCoordinate(0, 0.506145483),
                new Vector2(0, 0),
                1,
                new SpheroidEquatorialInvF(6378137, 298.25722356)
            );
            var input = new Point2(2847342.74, 71984.49);
            var expected = new GeographicCoordinate(-0.449108618, 0.493625066);

            var result = projection.GetInverse().TransformValue(input);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.003);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.002);
        }

        [Test]
        public void OsgbTest() {
            var projection = new TransverseMercator(
                new GeographicCoordinate(49 * Math.PI / 180, -2 * Math.PI / 180),
                new Vector2(400000, -100000),
                0.9996012717,
                new SpheroidEquatorialInvF(6377563.396, 299.32496)
            );
            var a = new Point2(651409.903, 313177.270);
            var b = new GeographicCoordinate(52.6576 * Math.PI / 180, 1.7179 * Math.PI / 180);

            var projected = projection.TransformValue(b);

            Assert.AreEqual(a.X, projected.X, 2);
            Assert.AreEqual(a.Y, projected.Y, 4);
        }

        [Test]
        public void OsgbInverseTest() {
            var projection = new TransverseMercator(
                new GeographicCoordinate(49 * Math.PI / 180, -2 * Math.PI / 180),
                new Vector2(400000, -100000),
                0.9996012717,
                new SpheroidEquatorialInvF(6377563.396, 299.32496)
            );
            var a = new Point2(651409.903, 313177.270);
            var b = new GeographicCoordinate(52.6576 * Math.PI / 180, 1.7179 * Math.PI / 180);

            var unProjected = projection.GetInverse().TransformValue(a);

            Assert.AreEqual(b.Latitude, unProjected.Latitude, 0.0000006);
            Assert.AreEqual(b.Longitude, unProjected.Longitude, 0.0000004);
        }

    }
}
