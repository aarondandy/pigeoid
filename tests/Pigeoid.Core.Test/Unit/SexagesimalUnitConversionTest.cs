using NUnit.Framework;
using Pigeoid.Unit;

namespace Pigeoid.Core.Test.Unit
{
    [TestFixture]
    public class SexagesimalUnitConversionTest
    {

        private class SimpleUnit : IUnit
        {

            public SimpleUnit(string name, string type = null) {
                Name = name;
                Type = type ?? "a-unit";
            }

            public string Name { get; set; }

            public string Type { get; set; }

            public IUnitConversion<double> ForwardOperation { get; set; }

            public IUnitConversionMap<double> ConversionMap {
                get {
                    if (null == ForwardOperation)
                        return null;
                    return new BinaryUnitConversionMap(ForwardOperation);
                }
            }
        }

        public SexagesimalDmsToDecimalDegreesConversion DmsConversion;
        public IUnitConversion<double> DmsInverse;
        public SexagesimalDmToDecimalDegreesConversion DmConversion;
        public IUnitConversion<double> DmInverse;

        [TestFixtureSetUp]
        public void TestFixtureSetUp() {
            DmsConversion = new SexagesimalDmsToDecimalDegreesConversion(new SimpleUnit("Sexagesimal DMS"), new SimpleUnit("degrees"));
            DmsInverse = DmsConversion.GetInverse();
            DmConversion = new SexagesimalDmToDecimalDegreesConversion(new SimpleUnit("Sexagesimal DM"), DmsConversion.To);
            DmInverse = DmConversion.GetInverse();
        }

        [Test]
        public void dms_positive_sample() {
            var expectedDecimalDegrees = 12.5050;
            var expectedSexagesimal = 12.3018;

            Assert.AreEqual(expectedDecimalDegrees, DmsConversion.TransformValue(expectedSexagesimal));
            Assert.AreEqual(expectedSexagesimal, DmsInverse.TransformValue(expectedDecimalDegrees));
        }

        [Test]
        public void dms_negative_sample() {
            var expectedDecimalDegrees = -12.5050;
            var expectedSexagesimal = -12.3018;

            Assert.AreEqual(expectedDecimalDegrees, DmsConversion.TransformValue(expectedSexagesimal));
            Assert.AreEqual(expectedSexagesimal, DmsInverse.TransformValue(expectedDecimalDegrees));
        }

        [Test]
        public void dm_positive_sample() {
            var expectedDecimalDegrees = 12.5050;
            var expectedSexagesimal = 12.303;

            Assert.AreEqual(expectedDecimalDegrees, DmConversion.TransformValue(expectedSexagesimal));
            Assert.AreEqual(expectedSexagesimal, DmInverse.TransformValue(expectedDecimalDegrees));
        }

        [Test]
        public void dm_negative_sample() {
            var expectedDecimalDegrees = -12.5050;
            var expectedSexagesimal = -12.303;

            Assert.AreEqual(expectedDecimalDegrees, DmConversion.TransformValue(expectedSexagesimal));
            Assert.AreEqual(expectedSexagesimal, DmInverse.TransformValue(expectedDecimalDegrees));
        }

    }
}
