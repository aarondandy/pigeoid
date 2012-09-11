using System;
using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.GoldData
{
	[TestFixture]
	public class PolarStereoTest
	{

		[Test]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_09.csv", 0.00001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_09a.csv", 0.00001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_09b.csv", 0.0000006, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_10.csv", 0.000001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_11.csv", 0.00001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_11a.csv", 0.00001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_11b.csv", 0.00001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_12.csv", 0.00001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_12a.csv", 0.00001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_13.csv", 0.00001, 0.000000001)]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_13a.csv", 0.00001, 0.000000001)]
		public void Test(string geoResourceName, string prjResourceName, double projectDelta, double unprojectDelta)
		{
			var latLonData = GoldData.GetReadyReader(geoResourceName);
			var prjData = GoldData.GetReadyReader(prjResourceName);

			var spheroid = GoldData.GenerateSpheroid(prjData["DATUM"]);
			PolarStereographic projection;
			if(null != prjData["SCALE FACTOR"])
			{
				projection = new PolarStereographicA(
					new GeographicCoordinate(
						Math.PI / 2.0,
						Double.Parse(prjData["LONGITUDE DOWN FROM POLE"]) * Math.PI / 180.0
					),
					Double.Parse(prjData["SCALE FACTOR"]),
					new Vector2(
						Double.Parse(prjData["FALSE EASTING"]),
						Double.Parse(prjData["FALSE NORTHING"])
					),
					spheroid
				);
			}
			else
			{
				var latSp = Double.Parse(prjData["LATITUDE OF TRUE SCALE"])*Math.PI/180.0;
				var originLat = latSp < 0 ? -Math.PI/2.0 : Math.PI/2.0;
				projection = new PolarStereographicB(
					new GeographicCoordinate(
						originLat,
						Double.Parse(prjData["LONGITUDE DOWN FROM POLE"])*Math.PI/180.0
					),
					latSp,
 					new Vector2(
						Double.Parse(prjData["FALSE EASTING"]),
						Double.Parse(prjData["FALSE NORTHING"])
					), 
					spheroid
				);
				;
			}

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
