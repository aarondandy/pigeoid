using NUnit.Framework;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;

namespace Pigeoid.Core.Test.Transformation
{
    [TestFixture]
    public class MolodenskyBadekasTransformationTest
    {

        [Test]
        public void EpsgExample2441Test() {
            var transform = new MolodenskyBadekasTransformation(
                new Vector3(-270.933, 115.599, -360.226),
                new Vector3(-0.000025530288, -0.000006001993, 0.000011543414),
                new Point3(2464351.59, -5783466.61, 974809.81),
                -5.109
            );
            var a = new Point3(2550408.96, -5749912.26, 1054891.11);
            var b = new Point3(2550138.46, -5749799.87, 1054530.82);

            var result = transform.TransformValue(a);

            Assert.AreEqual(b.X, result.X, 0.005);
            Assert.AreEqual(b.Y, result.Y, 0.0004);
            Assert.AreEqual(b.Z, result.Z, 0.006);
        }

        [Test]
        public void EpsgExample2441InverseTest() {
            var transform = new MolodenskyBadekasTransformation(
                new Vector3(-270.933, 115.599, -360.226),
                new Vector3(-0.000025530288, -0.000006001993, 0.000011543414),
                new Point3(2464351.59, -5783466.61, 974809.81),
                -5.109
            );
            var a = new Point3(2550408.96, -5749912.26, 1054891.11);
            var b = new Point3(2550138.46, -5749799.87, 1054530.82);

            var result = transform.GetInverse().TransformValue(b);

            Assert.AreEqual(a.X, result.X, 0.005);
            Assert.AreEqual(a.Y, result.Y, 0.0004);
            Assert.AreEqual(a.Z, result.Z, 0.006);
        }


    }
}
