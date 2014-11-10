using System;
using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.GoldData.Test
{

    [TestFixture]
    public class MercatorTest
    {
        [Test]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_5.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_5a.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_6.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_7.csv", 0.000002, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_8.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_8a.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_8b.csv", 0.0000005, 0.000000001)]
        public void Test(string geoResourceName, string mercResourceName, double projectDelta, double unprojectDelta) {
            var latLonData = GoldData.GetReadyReader(geoResourceName);
            var lccData = GoldData.GetReadyReader(mercResourceName);
            var latTrueScale = lccData["LATITUDE OF TRUE SCALE"];
            Mercator projection;
            if (null == latTrueScale) {
                projection = new Mercator(
                    Double.Parse(lccData["CENTRAL MERIDIAN"]) / 180 * Math.PI,
                    Double.Parse(lccData["SCALE FACTOR"]),
                    new Vector2(
                        Double.Parse(lccData["FALSE EASTING"]),
                        Double.Parse(lccData["FALSE NORTHING"])
                    ),
                    GoldData.GenerateSpheroid(lccData["DATUM"])
                );
            }
            else {
                projection = new Mercator(
                    new GeographicCoordinate(
                        Double.Parse(latTrueScale) / 180 * Math.PI,
                        Double.Parse(lccData["CENTRAL MERIDIAN"]) / 180 * Math.PI
                    ),
                    new Vector2(
                        Double.Parse(lccData["FALSE EASTING"]),
                        Double.Parse(lccData["FALSE NORTHING"])
                    ),
                    GoldData.GenerateSpheroid(lccData["DATUM"])
                );
            }

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
