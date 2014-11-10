using NUnit.Framework;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.Core.Test.Transformation
{
    [TestFixture]
    public class GeographicGeocentricTransformationTest
    {

        [Test]
        public void EpsgExample221InverseTest() {
            var transform = new GeographicGeocentricTransformation(new SpheroidEquatorialInvF(6378137.000, 298.2572236));
            var inputValue = new Point3(3771793.968, 140253.342, 5124304.349);
            var expected = new GeographicHeightCoordinate(0.939151102, 0.037167659, 73);

            var result = (transform.GetInverse() as ITransformation<Point3, GeographicCoordinate>).TransformValue(inputValue);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000006);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.0000000002);
        }

        [Test]
        public void EpsgExample221InverseHeightTest() {
            var transform = new GeographicGeocentricTransformation(new SpheroidEquatorialInvF(6378137.000, 298.2572236));
            var inputValue = new Point3(3771793.968, 140253.342, 5124304.349);
            var expected = new GeographicHeightCoordinate(0.939151102, 0.037167659, 73);

            var result = (transform.GetInverse() as ITransformation<Point3, GeographicHeightCoordinate>).TransformValue(inputValue);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000006);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.0000000002);
            Assert.AreEqual(expected.Height, result.Height, 0.00008);
        }

        [Test]
        public void EpsgExample221Test() {
            var transform = new GeographicGeocentricTransformation(new SpheroidEquatorialInvF(6378137.000, 298.2572236));
            var expected = new Point3(3771793.968, 140253.342, 5124304.349);
            var inputValue = new GeographicHeightCoordinate(0.939151102, 0.037167659, 73);

            var result = transform.TransformValue(inputValue);

            Assert.AreEqual(expected.X, result.X, 0.003);
            Assert.AreEqual(expected.Y, result.Y, 0.0006);
            Assert.AreEqual(expected.Z, result.Z, 0.003);
        }

        [Test]
        public void EpsgExample221WithoutHeightTest() {
            var transform = new GeographicGeocentricTransformation(new SpheroidEquatorialInvF(6378137.000, 298.2572236));
            var inputValue = new GeographicCoordinate(0.939151102, 0.037167659); // the actual point is 73 from the surface
            var expectedVector = new Vector3(3771793.968, 140253.342, 5124304.349);
            var expectedNormal = expectedVector.GetNormalized();

            var result = transform.TransformValue(new GeographicCoordinate(inputValue.Latitude, inputValue.Longitude));
            var resultVector = new Vector3(result); // so we can check that the normal is at least the same, as we remove height information
            var resultNormal = resultVector.GetNormalized();
            Assert.AreEqual(expectedNormal.X, resultNormal.X, 0.00000003);
            Assert.AreEqual(expectedNormal.Y, resultNormal.Y, 0.000000002);
            Assert.AreEqual(expectedNormal.Z, resultNormal.Z, 0.00000003);
            Assert.AreEqual(resultVector.GetMagnitude(), expectedVector.GetMagnitude() - 73.0, 0.0005); // it should be about 73 units from the expected height above the surface
        }

    }
}
