using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.Ogc;
using Pigeoid.Transformation;
using Vertesaur;

namespace Pigeoid.GoldData
{
	public static class GoldData
	{

		private static readonly Assembly ThisAssembly = Assembly.GetAssembly(typeof(GoldData));

		private static StreamReader GetEmbeddedStreamReader(string name)
		{
			var stream = ThisAssembly.GetManifestResourceStream(name)
				?? ThisAssembly.GetManifestResourceStream(typeof(GoldData).Namespace + ".Data." + name);
			
			if (null == stream)
				Assert.Inconclusive("Could not load resource: " + name, name);
			
			return new StreamReader(stream);
		}

		public static GeoTransGoldDataReader GetReadyReader(string name)
		{
			var reader = new GeoTransGoldDataReader(GetEmbeddedStreamReader(name));
			if (!reader.Read())
				Assert.Inconclusive("Could not read header: " + name, name);

			return reader;
		}

		public static ISpheroidInfo GenerateSpheroid(string name)
		{
			if ("WGE".Equals(name.ToUpper()) || "WE".Equals(name.ToUpper()))
				return new OgcSpheroid(new SpheroidEquatorialInvF(6378137, 298.257223563), name, OgcLinearUnit.DefaultMeter);

			Assert.Inconclusive("Spheroid not found: " + name, name);
			return null;
		}

		public static IDatumGeodetic GenerateDatum(string name) {
			var spheroid = GenerateSpheroid(name);
			return new OgcDatumHorizontal(
				spheroid.Name,
				spheroid,
				new OgcPrimeMeridian("Greenwich", 0),
				new Helmert7Transformation(Vector3.Zero)
			);
		}

		public static ICrs GetCrs(GeoTransGoldDataReader reader) {
			var coordinatesName = reader["COORDINATES"];
			if (String.IsNullOrEmpty(coordinatesName))
				coordinatesName = reader["PROJECTION"];

			switch(coordinatesName) {
			case "Geodetic":
				return CreateGeographicCrs(reader);
			case "Lambert Conformal Conic (1 parallel)":
				return CreateLcc1Sp(reader);
			default:
				throw new NotSupportedException("Not supported: " + coordinatesName);
			}
		}

		private static ICrs CreateLcc1Sp(GeoTransGoldDataReader reader) {
			var datum = GenerateDatum(reader["DATUM"]);

			return new OgcCrsProjected(
				reader["PROJECTION"],
				new OgcCrsGeographic(
					datum.Name,
					datum,
					datum.PrimeMeridian.Unit,
					new[] {
						new OgcAxis("Latitude", OgcOrientationType.East),
						new OgcAxis("Longitude", OgcOrientationType.North) 
					}
				),
 				new CoordinateOperationInfo(
					"Lambert_Conformal_Conic_1SP",
					new INamedParameter[] {
						new NamedParameter<double>("latitude_of_origin",Double.Parse(reader["ORIGIN LATITUDE"]) / 180 * Math.PI),
						new NamedParameter<double>("central_meridian", Double.Parse(reader["CENTRAL MERIDIAN"]) / 180 * Math.PI),
						new NamedParameter<double>("scale_factor", Double.Parse(reader["SCALE FACTOR"])),
						new NamedParameter<double>("false_easting", Double.Parse(reader["FALSE EASTING"])),
						new NamedParameter<double>("false_northing", Double.Parse(reader["FALSE NORTHING"]))
					}
				),
 				OgcLinearUnit.DefaultMeter,
 				new IAxis[] {
 					new OgcAxis("Easting", OgcOrientationType.East), 
					new OgcAxis("Northing", OgcOrientationType.North)
 				}
			);
			
		}

		private static ICrs CreateGeographicCrs(GeoTransGoldDataReader reader) {
			var datum = GenerateDatum(reader["DATUM"]);
			return new OgcCrsGeographic(
				reader["COORDINATES"],
				datum,
				datum.PrimeMeridian.Unit,
				new[] {
					new OgcAxis("Latitude", OgcOrientationType.East),
					new OgcAxis("Longitude", OgcOrientationType.North) 
				}
			);
		}

	}
}
