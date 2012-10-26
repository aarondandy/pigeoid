using System;
using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.GoldData
{
	[TestFixture]
	public class LambertConicConformal2SpTest
	{

		[Test]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_16.csv", 0.0000006, 0.000000000066)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_16a.csv", 0.0000006, 0.000000000071)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_17.csv", 0.0000006, 0.00000000002)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_18.csv", 0.00015, 1.3948895055401E-09)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_19.csv", 0.00000051, 0.000000000017)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_20.csv", 0.000006, 0.00000000005)]
		public void Test(string geoResourceName, string lccResourceName, double projectDelta, double unprojectDelta)
		{

			var latLonData = GoldData.GetReadyReader(geoResourceName);
			var lccData = GoldData.GetReadyReader(lccResourceName);

			var projection = new LambertConicConformal2Sp(
				new GeographicCoordinate(
					Double.Parse(lccData["ORIGIN LATITUDE"]) / 180 * Math.PI,
					Double.Parse(lccData["CENTRAL MERIDIAN"]) / 180 * Math.PI
				), 
				Double.Parse(lccData["STANDARD PARALLEL ONE"]) / 180 * Math.PI,
				Double.Parse(lccData["STANDARD PARALLEL TWO"]) / 180 * Math.PI,
				new Vector2(
					Double.Parse(lccData["FALSE EASTING"]),
					Double.Parse(lccData["FALSE NORTHING"])
				),
				GoldData.GenerateSpheroid(lccData["DATUM"])
			);

			var inverse = projection.GetInverse();

			while (latLonData.Read() && lccData.Read())
			{
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
