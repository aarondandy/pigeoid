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
    public class LambertConicConformal1SpTest
    {

        [Test]
        public void EpsgExample1312() {
            var projection = new LambertConicConformal1Sp(
                new GeographicCoordinate(0.31415927, -1.34390352),
                1,
                new Vector2(250000, 150000),
                new SpheroidEquatorialInvF(6378206.400, 294.97870));
            var input = new GeographicCoordinate(0.31297535, -1.34292061);
            var expected = new Point2(255966.58, 142493.51);

            var result = projection.TransformValue(input);

            Assert.AreEqual(expected.X, result.X, 0.006);
            Assert.AreEqual(expected.Y, result.Y, 0.04);
        }

        [Test]
        public void EpsgExample1312Inverse() {
            var projection = new LambertConicConformal1Sp(
                new GeographicCoordinate(0.31415927, -1.34390352),
                1,
                new Vector2(250000, 150000),
                new SpheroidEquatorialInvF(6378206.400, 294.97870));
            Assert.That(projection.HasInverse);
            var inverse = projection.GetInverse();

            var expected = new GeographicCoordinate(0.31297535, -1.34292061);
            var input = new Point2(255966.58, 142493.51);

            var result = inverse.TransformValue(input);

            Assert.AreEqual(expected.Latitude, result.Latitude, 0.00000001);
            Assert.AreEqual(expected.Longitude, result.Longitude, 0.00000001);
        }

    }
}
