using System;
using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.GoldData
{
	[TestFixture]
	public class NeysTest
	{

		[Test]
		[TestCase("Wgs84.Lat_Lon.csv","Wgs84.Ney_24.csv",800000,0.2)] // terrible!
		[TestCase("Wgs84.Lat_Lon.csv","Wgs84.Ney_25.csv",900000,0.2)] // terrible!!!
		public void Test(string geoResourceName, string neyResourceName, double projectDelta, double unprojectDelta)
		{
			var latLonData = GoldData.GetReadyReader(geoResourceName);
			var prjData = GoldData.GetReadyReader(neyResourceName);


			var projection = new NeysProjection(
				new GeographicCoordinate(
					Double.Parse(prjData["ORIGIN LATITUDE"]) * Math.PI / 180.0,
					Double.Parse(prjData["CENTRAL MERIDIAN"]) * Math.PI / 180.0
				),
				Double.Parse(prjData["STANDARD PARALLEL ONE"]) * Math.PI / 180.0,
				new Vector2(
					Double.Parse(prjData["FALSE EASTING"]),
					Double.Parse(prjData["FALSE NORTHING"])
				),
				GoldData.GenerateSpheroid(prjData["DATUM"])
			);

			var inverse = projection.GetInverse();

			while (latLonData.Read() && prjData.Read())
			{
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
