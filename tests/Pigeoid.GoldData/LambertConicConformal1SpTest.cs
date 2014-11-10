using System;
using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.GoldData
{
    [TestFixture]
    public class LambertConicConformal1SpTest
    {

        [Test]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_14.csv", 0.000001, 0.00000000011)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_14a.csv", 0.000001, 0.00000000011)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_15.csv", 0.000001, 0.00000000011)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_21.csv", 0.0035, 0.0000000007)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_22.csv", 0.0005, 0.000000003)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_23.csv", 0.000001, 0.00000000011)]
        public void Test(string geoResourceName, string lccResourceName, double projectDelta, double unprojectDelta) {

            var latLonData = GoldData.GetReadyReader(geoResourceName);
            var lccData = GoldData.GetReadyReader(lccResourceName);

            var projection = new LambertConicConformal1Sp(
                new GeographicCoordinate(
                    Double.Parse(lccData["ORIGIN LATITUDE"]) / 180 * Math.PI,
                    Double.Parse(lccData["CENTRAL MERIDIAN"]) / 180 * Math.PI
                ),
                Double.Parse(lccData["SCALE FACTOR"]),
                new Vector2(
                    Double.Parse(lccData["FALSE EASTING"]),
                    Double.Parse(lccData["FALSE NORTHING"])
                ),
                GoldData.GenerateSpheroid(lccData["DATUM"])
            );

            var inverse = projection.GetInverse();

            while (latLonData.Read() && lccData.Read()) {
                var coord = latLonData.CurrentLatLon();
                var coordRads = latLonData.CurrentLatLonRadians();
                var expected = lccData.CurrentPoint2D();

                var projected = projection.TransformValue(coordRads);
                Assert.AreEqual(expected.X, projected.X, projectDelta);
                Assert.AreEqual(expected.Y, projected.Y, projectDelta);

                var unProjected = inverse.TransformValue(expected);
                unProjected = new GeographicCoordinate(unProjected.Latitude * 180.0 / Math.PI, unProjected.Longitude * 180.0 / Math.PI);
                Assert.AreEqual(coord.Latitude, unProjected.Latitude, unprojectDelta);
                Assert.AreEqual(coord.Longitude, unProjected.Longitude, unprojectDelta);
            }
        }

    }
}
