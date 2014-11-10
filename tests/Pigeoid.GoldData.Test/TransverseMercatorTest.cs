using System;
using NUnit.Framework;
using Pigeoid.CoordinateOperation.Projection;
using Vertesaur;

namespace Pigeoid.GoldData.Test
{
    [TestFixture]
    public class TransverseMercatorTest
    {

        [Test]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.TransMerc_26.csv", 0.009, 0.0000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.TransMerc_26a.csv", 0.009, 0.0000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.TransMerc_27.csv", 0.04, 0.0000001)]
        public void Test(string geoResourceName, string prjResourceName, double projectDelta, double unprojectDelta) {
            var latLonData = GoldData.GetReadyReader(geoResourceName);
            var prjData = GoldData.GetReadyReader(prjResourceName);

            var spheroid = GoldData.GenerateSpheroid(prjData["DATUM"]);
            var projection = new TransverseMercator(
                new GeographicCoordinate(
                    Double.Parse(prjData["ORIGIN LATITUDE"]) * Math.PI / 180.0,
                    Double.Parse(prjData["CENTRAL MERIDIAN"]) * Math.PI / 180.0
                ),
                new Vector2(
                    Double.Parse(prjData["FALSE EASTING"]),
                    Double.Parse(prjData["FALSE NORTHING"])
                ),
                Double.Parse(prjData["SCALE FACTOR"]),
                spheroid
            );

            var inverse = projection.GetInverse();

            while (latLonData.Read() && prjData.Read()) {
                var coord = latLonData.CurrentLatLon();
                var coordRads = latLonData.CurrentLatLonRadians();
                var expected = prjData.CurrentPoint2D();

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
