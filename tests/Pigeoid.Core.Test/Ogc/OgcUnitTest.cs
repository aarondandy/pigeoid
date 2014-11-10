using System;
using NUnit.Framework;
using Pigeoid.Ogc;
using Pigeoid.Unit;

namespace Pigeoid.Core.Test.Ogc
{
    [TestFixture]
    public class OgcUnitTest
    {

        [Test]
        public void SiAndImperialLengthTest() {
            var km = new OgcLinearUnit("kilometer", 1000);
            var mile = new OgcLinearUnit("mile", 1609.344);
            var conversion = SimpleUnitConversionGenerator.FindConversion(km, mile);
            Assert.IsNotNull(conversion);
            Assert.AreEqual(0.621371192, conversion.TransformValue(1), 0.00001);
        }

        [Test]
        public void GradToDegreeTest() {
            var grad = new OgcAngularUnit("grad", Math.PI / 200);
            var degree = new OgcAngularUnit("degree", Math.PI / 180);
            var conversion = SimpleUnitConversionGenerator.FindConversion(grad, degree);
            Assert.IsNotNull(conversion);
            Assert.AreEqual(0.9, conversion.TransformValue(1), 0.00001);
        }

    }
}
