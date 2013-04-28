using NUnit.Framework;
using Pigeoid.CoordinateOperation.Transformation;

namespace Pigeoid.Core.Test.Transformation
{
    [TestFixture]
    public class MadridEd50PolynomialTest
    {

        [Test]
        public void example_2_3_1_3() {
            var transformation = new MadridEd50Polynomial(
                11.328779,
                -0.1674,
                -0.03852,
                0.0000379,
                -13276.58,
                2.5079425,
                0.08352,
                -0.00864,
                -0.0000038
            );

            var input = new GeographicCoordinate(42.647992, 3.659603);
            var expectedResult = new GeographicCoordinate(42.649117, -0.026658);

            var result = transformation.TransformValue(input);

            Assert.AreEqual(expectedResult.Latitude, result.Latitude, 0.0000005);
            Assert.AreEqual(expectedResult.Longitude, result.Longitude, 0.0000006);
        }

    }
}
