using System;
using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{

    [TestFixture]
    public class LambertAzimuthalEqualAreaTest
    {

        /// <summary>
        /// Lambert Azimuthal Equal Area
        /// </summary>
        [Test]
        public void epsg_example_1_3_11() {
            var projection = new LambertAzimuthalEqualAreaOblique(
                new GeographicCoordinate(0.907571211, 0.174532925),
                new Vector2(4321000, 3210000),
                new SpheroidEquatorialInvF(6378137, 298.2572221));

            var projectedExpected = new Point2(3962799.45, 2999718.85);
            var geographicExpected = new GeographicCoordinate(0.872664626, 0.087266463);

            var projectedActual = projection.TransformValue(geographicExpected);
            Assert.AreEqual(projectedExpected.X, projectedActual.X, 0.004);
            Assert.AreEqual(projectedExpected.Y, projectedActual.Y, 0.004);

            var inverse = projection.GetInverse();
            Assert.IsNotNull(inverse);

            var geographicActual = inverse.TransformValue(projectedExpected);
            Assert.AreEqual(geographicExpected.Latitude, geographicActual.Latitude, 0.000001);
            Assert.AreEqual(geographicExpected.Longitude, geographicActual.Longitude, 0.00000003);
        }

        [Test]
        public void map_proj_working_manual_lambert_az_eq_area_spherical_example() {
            var projection = new LambertAzimuthalEqualAreaSpherical(
                new GeographicCoordinate(0, 0),
                new Vector2(0, 0),
                new Sphere(1));
            Assert.That(projection.HasInverse);
            var inverse = projection.GetInverse();

            var projectedExpected = new Point2(0.61040, 0.54826);
            var geographicExpected = new GeographicCoordinate(30 * Math.PI / 180.0, 40 * Math.PI / 180.0);

            var projectedActual = projection.TransformValue(geographicExpected);
            Assert.AreEqual(projectedExpected.X, projectedActual.X, 0.00001);
            Assert.AreEqual(projectedExpected.Y, projectedActual.Y, 0.000003);

            var geographicActual = inverse.TransformValue(projectedExpected);
            Assert.AreEqual(geographicExpected.Latitude, geographicActual.Latitude, 0.00001);
            Assert.AreEqual(geographicExpected.Longitude, geographicActual.Longitude, 0.000004);
        }

    }
}
