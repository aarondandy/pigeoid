using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Pigeoid.CoordinateOperation;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Ogc;
using Vertesaur;
using Pigeoid.Unit;
using System.Collections.Generic;

namespace Pigeoid.GoldData
{
    public static class GoldData
    {

        private static readonly Assembly ThisAssembly = Assembly.GetAssembly(typeof(GoldData));

        private static StreamReader GetEmbeddedStreamReader(string name) {
            var stream = ThisAssembly.GetManifestResourceStream(name)
                ?? ThisAssembly.GetManifestResourceStream(typeof(GoldData).Namespace + ".Data." + name);

            if (null == stream)
                Assert.Inconclusive("Could not load resource: " + name, name);

            return new StreamReader(stream);
        }

        public static GeoTransGoldDataReader GetReadyReader(string name) {
            var reader = new GeoTransGoldDataReader(GetEmbeddedStreamReader(name));
            if (!reader.Read())
                Assert.Inconclusive("Could not read header: " + name, name);

            return reader;
        }

        public static ISpheroidInfo GenerateSpheroid(string name) {
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

            switch (coordinatesName) {
            case "Geodetic":
                return CreateGeographicCrs(reader);
            case "Lambert Conformal Conic (1 parallel)":
                return CreateLcc1Sp(reader);
            case "Lambert Conformal Conic (2 parallel)":
                return CreateLcc2Sp(reader);
            case "Mercator":
                return CreateMercator(reader);
            case "Polar Stereographic":
                return CreatePolarStereographic(reader);
            case "Transverse Mercator":
                return CreateTMerc(reader);
            default:
                throw new NotSupportedException("Not supported: " + coordinatesName);
            }
        }

        private static ICrs CreateTMerc(GeoTransGoldDataReader reader) {
            var datum = GenerateDatum(reader["DATUM"]);
            var linearUnit = OgcLinearUnit.DefaultMeter;
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
                    "Transverse Mercator",
                    new INamedParameter[] {
                        new NamedParameter<double>("latitude_of_origin",Double.Parse(reader["ORIGIN LATITUDE"]), OgcAngularUnit.DefaultDegrees),
                        new NamedParameter<double>("central_meridian", Double.Parse(reader["CENTRAL MERIDIAN"]), OgcAngularUnit.DefaultDegrees),
                        new NamedParameter<double>("scale_factor", Double.Parse(reader["SCALE FACTOR"]), ScaleUnitUnity.Value),
                        new NamedParameter<double>("false_easting", Double.Parse(reader["FALSE EASTING"]),linearUnit),
                        new NamedParameter<double>("false_northing", Double.Parse(reader["FALSE NORTHING"]),linearUnit)
                    }
                ),
                linearUnit,
                new IAxis[] {
                    new OgcAxis("Easting", OgcOrientationType.East), 
                    new OgcAxis("Northing", OgcOrientationType.North)
                }
            );
        }

        private static ICrs CreatePolarStereographic(GeoTransGoldDataReader reader) {
            var datum = GenerateDatum(reader["DATUM"]);
            var linearUnit = OgcLinearUnit.DefaultMeter;
            var scaleFactor = reader["SCALE FACTOR"];
            var parameters = new List<INamedParameter> {
                new NamedParameter<double>("longitude_down_from_pole", Double.Parse(reader["LONGITUDE DOWN FROM POLE"]), OgcAngularUnit.DefaultDegrees),
                new NamedParameter<double>("false_easting", Double.Parse(reader["FALSE EASTING"]),linearUnit),
                new NamedParameter<double>("false_northing", Double.Parse(reader["FALSE NORTHING"]),linearUnit)
            };

            if (null != scaleFactor) {
                parameters.Add(new NamedParameter<double>("latitude_of_true_scale", 90, OgcAngularUnit.DefaultDegrees));
                parameters.Add(new NamedParameter<double>("scale_factor", Double.Parse(scaleFactor), ScaleUnitUnity.Value));
            }
            else {
                var latSp = Double.Parse(reader["LATITUDE OF TRUE SCALE"]);
                parameters.Add(new NamedParameter<double>("standard_parallel", latSp, OgcAngularUnit.DefaultDegrees));
                parameters.Add(new NamedParameter<double>("latitude_of_true_scale", latSp < 0 ? -90 : 90, OgcAngularUnit.DefaultDegrees));
            }

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
                    "Polar Stereographic",
                    parameters
                ),
                linearUnit,
                new IAxis[] {
                    new OgcAxis("Easting", OgcOrientationType.East), 
                    new OgcAxis("Northing", OgcOrientationType.North)
                }
            );
        }

        private static ICrs CreateMercator(GeoTransGoldDataReader reader) {
            var datum = GenerateDatum(reader["DATUM"]);
            var linearUnit = OgcLinearUnit.DefaultMeter;
            var latTrueScale = reader["LATITUDE OF TRUE SCALE"];
            var parameters = new List<INamedParameter> {
                new NamedParameter<double>("central_meridian", Double.Parse(reader["CENTRAL MERIDIAN"]), OgcAngularUnit.DefaultDegrees),
                new NamedParameter<double>("false_easting", Double.Parse(reader["FALSE EASTING"]),linearUnit),
                new NamedParameter<double>("false_northing", Double.Parse(reader["FALSE NORTHING"]),linearUnit)
            };

            if (null == latTrueScale) {
                parameters.Add(new NamedParameter<double>("scale_factor", Double.Parse(reader["SCALE FACTOR"]), ScaleUnitUnity.Value));
            }
            else {
                parameters.Add(new NamedParameter<double>("latitude_of_true_scale", Double.Parse(latTrueScale), OgcAngularUnit.DefaultDegrees));
            }

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
                    "Mercator",
                    parameters
                ),
                linearUnit,
                new IAxis[] {
                    new OgcAxis("Easting", OgcOrientationType.East), 
                    new OgcAxis("Northing", OgcOrientationType.North)
                }
            );
        }

        private static ICrs CreateLcc1Sp(GeoTransGoldDataReader reader) {
            var datum = GenerateDatum(reader["DATUM"]);
            var linearUnit = OgcLinearUnit.DefaultMeter;
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
                        new NamedParameter<double>("latitude_of_origin",Double.Parse(reader["ORIGIN LATITUDE"]), OgcAngularUnit.DefaultDegrees),
                        new NamedParameter<double>("central_meridian", Double.Parse(reader["CENTRAL MERIDIAN"]), OgcAngularUnit.DefaultDegrees),
                        new NamedParameter<double>("scale_factor", Double.Parse(reader["SCALE FACTOR"]), ScaleUnitUnity.Value),
                        new NamedParameter<double>("false_easting", Double.Parse(reader["FALSE EASTING"]),linearUnit),
                        new NamedParameter<double>("false_northing", Double.Parse(reader["FALSE NORTHING"]),linearUnit)
                    }
                ),
                linearUnit,
                new IAxis[] {
                    new OgcAxis("Easting", OgcOrientationType.East), 
                    new OgcAxis("Northing", OgcOrientationType.North)
                }
            );

        }

        private static ICrs CreateLcc2Sp(GeoTransGoldDataReader reader) {
            var datum = GenerateDatum(reader["DATUM"]);
            var linearUnit = OgcLinearUnit.DefaultMeter;
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
                    "Lambert_Conformal_Conic_2SP",
                    new INamedParameter[] {
                        new NamedParameter<double>("latitude_of_origin",Double.Parse(reader["ORIGIN LATITUDE"]), OgcAngularUnit.DefaultDegrees),
                        new NamedParameter<double>("central_meridian", Double.Parse(reader["CENTRAL MERIDIAN"]), OgcAngularUnit.DefaultDegrees),
                        new NamedParameter<double>("parallel_1", Double.Parse(reader["STANDARD PARALLEL ONE"]), OgcAngularUnit.DefaultDegrees),
                        new NamedParameter<double>("parallel_2", Double.Parse(reader["STANDARD PARALLEL TWO"]), OgcAngularUnit.DefaultDegrees),
                        new NamedParameter<double>("false_easting", Double.Parse(reader["FALSE EASTING"]),linearUnit),
                        new NamedParameter<double>("false_northing", Double.Parse(reader["FALSE NORTHING"]),linearUnit)
                    }
                ),
                linearUnit,
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
