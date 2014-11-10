using NUnit.Framework;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;

namespace Pigeoid.Core.Test.Transformation
{
    [TestFixture]
    public class Helmert7TransformTest
    {

        [Test]
        public void EpsgExample2432Test() {
            var wgs72 = new Point3(3657660.66, 255768.55, 5201382.11);
            var wgs84 = new Point3(3657660.78, 255778.43, 5201387.75);
            var transform = Helmert7Transformation.CreateCoordinateFrameRotationFormat(
                new Vector3(0, 0, 4.5),
                new Vector3(0, 0, 0.000002685868),
                0.219
            );

            var result = transform.TransformValue(wgs72);

            Assert.AreEqual(wgs84.X, result.X, 1);
            Assert.AreEqual(wgs84.Y, result.Y, 10);
            Assert.AreEqual(wgs84.Z, result.Z, 0.001);
        }

        [Test]
        public void EpsgExample2432InverseTest() {
            var wgs72 = new Point3(3657660.66, 255768.55, 5201382.11);
            var wgs84 = new Point3(3657660.78, 255778.43, 5201387.75);
            var transform = Helmert7Transformation.CreateCoordinateFrameRotationFormat(
                new Vector3(0, 0, 4.5),
                new Vector3(0, 0, 0.000002685868),
                0.219
            );

            var result = transform.GetInverse().TransformValue(wgs84);

            Assert.AreEqual(wgs72.X, result.X, 0.7);
            Assert.AreEqual(wgs72.Y, result.Y, 10);
            Assert.AreEqual(wgs72.Z, result.Z, 0.0009);
        }

    }
}
