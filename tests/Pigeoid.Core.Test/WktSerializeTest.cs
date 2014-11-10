using System;
using NUnit.Framework;
using Pigeoid.CoordinateOperation;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Core.Test.Mock;
using Pigeoid.Ogc;
using Vertesaur;

namespace Pigeoid.Core.Test
{
    [TestFixture]
    public class WktSerializeTest
    {

        public readonly WktSerializer Default = new WktSerializer();
        public readonly WktSerializer Pretty = new WktSerializer(new WktOptions { Pretty = true });
        public readonly WktSerializer Throws = new WktSerializer(new WktOptions { ThrowOnError = true });

        public WktSerializeTest() {
            AllSerializers = new[] { Default, Pretty, Throws };
            SerializerNames = new[] { "Default", "Pretty", "Throws" };
        }

        public WktSerializer[] AllSerializers { get; private set; }

        public string[] SerializerNames { get; private set; }

        [Test]
        public void SerializeAuthorityTest([ValueSource("AllSerializers")] WktSerializer serializer) {
            Assert.AreEqual(@"AUTHORITY[""EPSG"",""9001""]", serializer.Serialize(new AuthorityTag("EPSG", "9001")));
            Assert.AreEqual(@"AUTHORITY[""AbC"",""""]", serializer.Serialize(new AuthorityTag("AbC", null)));
            Assert.AreEqual(@"AUTHORITY[""DEF"",""ghi""]", serializer.Serialize(new AuthorityTag("DEF", "ghi")));
        }

        [Test]
        public void SerializeNamedParameterTest([ValueSource("AllSerializers")] WktSerializer serializer) {
            Assert.AreEqual(@"PARAMETER[""a"",1234]", serializer.Serialize(new NamedParameter<double>("a", 1234)));
            Assert.AreEqual(@"PARAMETER[""b"",""test""]", serializer.Serialize(new NamedParameter<string>("B", "test")));
            Assert.AreEqual(@"PARAMETER[""blank"",""""]", serializer.Serialize(new NamedParameter<object>("blank", null)));
        }

        [Test]
        public void SerializeSpheroidTest([ValueSource("AllSerializers")] WktSerializer serializer) {
            Assert.AreEqual(
                @"SPHEROID[""round"",12345,278,AUTHORITY[""PIGEOID"",""?!#$""]]",
                serializer.Serialize(new OgcSpheroid(
                    new SpheroidEquatorialInvF(12345, 278),
                    "round",
                    OgcLinearUnit.DefaultMeter,
                    new AuthorityTag("PIGEOID", "?!#$")
                ))
            );
        }

        [Test]
        public void SerializePrimerMeridianTest([ValueSource("AllSerializers")] WktSerializer serializer) {
            Assert.AreEqual(
                @"PRIMEM[""ummm"",0.1,AUTHORITY[""PIGEOID"",""?!#$""]]",
                serializer.Serialize(new OgcPrimeMeridian(
                    "ummm",
                    0.1,
                    new AuthorityTag("PIGEOID", "?!#$")
                ))
            );
        }

        [Test]
        public void SerializeToWgs84Test([ValueSource("AllSerializers")] WktSerializer serializer) {
            Assert.AreEqual(
                @"TOWGS84[1,2,3,4,5,6,7]",
                serializer.Serialize(new Helmert7Transformation(
                    new Vector3(1, 2, 3),
                    new Vector3(4, 5, 6),
                    7
                ))
            );
        }

        [Test]
        public void SerializeDatumTest([ValueSource("AllSerializers")] WktSerializer serializer) {
            Assert.AreEqual(
                "VERT_DATUM[\"test\",2001,AUTHORITY[\"EPSG\",\"1234\"]]",
                Default.Serialize(new OgcDatum(
                    "test",
                    OgcDatumType.VerticalOrthometric,
                    new AuthorityTag("EPSG", "1234")
                ))
            );
        }

        [Test]
        public void SerializeUomTest([ValueSource("AllSerializers")] WktSerializer serializer) {
            Assert.AreEqual(
                "UNIT[\"test\",1.2,AUTHORITY[\"EPSG\",\"1234\"]]",
                Default.Serialize(new OgcLinearUnit(
                    "test",
                    1.2,
                    new AuthorityTag("EPSG", "1234")
                ))
            );
        }

        [Test]
        public void SerializePrettyConcatMathTransformTest() {
            var input = new ConcatenatedCoordinateOperationInfo(
                new[] {
                    new CoordinateOperationInfo(
                        "Helmert 7 Parameter Transformation",
                        new INamedParameter[]{
                            new NamedParameter<double>("dx",1),
                            new NamedParameter<double>("dy",2),
                            new NamedParameter<double>("dz",3),
                            new NamedParameter<double>("rx",4),
                            new NamedParameter<double>("ry",5),
                            new NamedParameter<double>("rz",6),
                            new NamedParameter<double>("m", 7)
                        }
                    ),
                    new CoordinateOperationInfo(
                        "Thing 1",
                        new INamedParameter[]{
                            new NamedParameter<double>("semi major", 6378137),
                            new NamedParameter<double>("semi minor", 6356752.31414035)
                        },
                        new OgcCoordinateOperationMethodInfo(
                            "Ellipsoid To Geocentric"
                        )
                    ),
                    new CoordinateOperationInfo(
                        "Thing 3",
                        new INamedParameter[]{
                            new NamedParameter<double>("semi_major", 6378206.4),
                            new NamedParameter<double>("semi_minor", 6356583.8)
                        },
                        new OgcCoordinateOperationMethodInfo(
                            "Ellipsoid_To_Geocentric"
                        )
                    ).GetInverse()
                }
            );
            var expectedPretty = String.Join(Environment.NewLine, new[]{
                "CONCAT_MT[",
                "\tPARAM_MT[\"Helmert_7_Parameter_Transformation\",",
                "\t\tPARAMETER[\"dx\",1],",
                "\t\tPARAMETER[\"dy\",2],",
                "\t\tPARAMETER[\"dz\",3],",
                "\t\tPARAMETER[\"rx\",4],",
                "\t\tPARAMETER[\"ry\",5],",
                "\t\tPARAMETER[\"rz\",6],",
                "\t\tPARAMETER[\"m\",7]],",
                "\tPARAM_MT[\"Ellipsoid_To_Geocentric\",",
                "\t\tPARAMETER[\"semi_major\",6378137],",
                "\t\tPARAMETER[\"semi_minor\",6356752.31414035]],",
                "\tINVERSE_MT[",
                "\t\tPARAM_MT[\"Ellipsoid_To_Geocentric\",",
                "\t\t\tPARAMETER[\"semi_major\",6378206.4],",
                "\t\t\tPARAMETER[\"semi_minor\",6356583.8]]]]"
            });
            var expectedDefault = expectedPretty.Replace(Environment.NewLine, "").Replace("\t", "");

            Assert.AreEqual(expectedDefault, Default.Serialize(input));
            Assert.AreEqual(expectedPretty, Pretty.Serialize(input));
        }

        [Test]
        public void SerializeOgcWktSample() {
            // NOTE: the WKT is slightly more condensed than the OGC WKT... because that is how I like it
            var expectedPretty = String.Join(Environment.NewLine, new[]{
                "COMPD_CS[\"OSGB36 / British National Grid + ODN\",",
                "\tPROJCS[\"OSGB 1936 / British National Grid\",",
                "\t\tGEOGCS[\"OSGB 1936\",",
                "\t\t\tDATUM[\"OSGB_1936\",",
                "\t\t\t\tSPHEROID[\"Airy 1830\",6377563.396,299.3249646,AUTHORITY[\"EPSG\",\"7001\"]],",
                "\t\t\t\tTOWGS84[375,-111,431,0,0,0,0],",
                "\t\t\t\tAUTHORITY[\"EPSG\",\"6277\"]],",
                "\t\t\tPRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],",
                "\t\t\tUNIT[\"DMSH\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9108\"]],",
                "\t\t\tAXIS[\"Lat\",NORTH],",
                "\t\t\tAXIS[\"Long\",EAST],",
                "\t\t\tAUTHORITY[\"EPSG\",\"4277\"]],",
                "\t\tPROJECTION[\"Transverse_Mercator\"],",
                "\t\tPARAMETER[\"latitude_of_origin\",49],",
                "\t\tPARAMETER[\"central_meridian\",-2],",
                "\t\tPARAMETER[\"scale_factor\",0.999601272],",
                "\t\tPARAMETER[\"false_easting\",400000],",
                "\t\tPARAMETER[\"false_northing\",-100000],",
                "\t\tUNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],",
                "\t\tAXIS[\"E\",EAST],",
                "\t\tAXIS[\"N\",NORTH],",
                "\t\tAUTHORITY[\"EPSG\",\"27700\"]],",
                "\tVERT_CS[\"Newlyn\",",
                "\t\tVERT_DATUM[\"Ordnance Datum Newlyn\",2005,AUTHORITY[\"EPSG\",\"5101\"]],",
                "\t\tUNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],",
                "\t\tAXIS[\"Up\",UP],",
                "\t\tAUTHORITY[\"EPSG\",\"5701\"]],",
                "\tAUTHORITY[\"EPSG\",\"7405\"]]"
            });
            var expectedFlat = expectedPretty
                .Replace("\t", "")
                .Replace(Environment.NewLine, "");

            var input = new OgcCrsCompound(
                "OSGB36 / British National Grid + ODN",
                new OgcCrsProjected(
                    "OSGB 1936 / British National Grid",
                    new OgcCrsGeographic(
                        "OSGB 1936",
                        new OgcDatumHorizontal(
                            "OSGB_1936",
                            new OgcSpheroid(
                                new SpheroidEquatorialInvF(6377563.396, 299.3249646),
                                "Airy 1830",
                                OgcLinearUnit.DefaultMeter,
                                new AuthorityTag("EPSG", "7001")
                            ),
                            new OgcPrimeMeridian("Greenwich", 0, new AuthorityTag("EPSG", "8901")),
                            new Helmert7Transformation(
                                new Vector3(375, -111, 431),
                                new Vector3(0, 0, 0),
                                0
                            ),
                            new AuthorityTag("EPSG", "6277")
                        ),
                        new OgcAngularUnit("DMSH", 0.0174532925199433, new AuthorityTag("EPSG", "9108")),
                        new IAxis[] {
                            new OgcAxis("Lat", OgcOrientationType.North),
                            new OgcAxis("Long", OgcOrientationType.East) 
                        },
                        new AuthorityTag("EPSG", "4277")
                    ),
                    new CoordinateOperationInfo(
                        "British National Grid",
                        new INamedParameter[] {
                            new NamedParameter<double>("latitude of origin",49),
                            new NamedParameter<double>("central meridian",-2),
                            new NamedParameter<double>("scale factor",0.999601272),
                            new NamedParameter<double>("false easting",400000),
                            new NamedParameter<double>("false northing",-100000)
                        },
                        new OgcCoordinateOperationMethodInfo(
                            "Transverse Mercator",
                            new AuthorityTag("EPSG", "9807")
                        ),
                        new AuthorityTag("EPSG", "19916")
                    ),
                    new OgcLinearUnit("metre", 1, new AuthorityTag("EPSG", "9001")),
                    new IAxis[] {
                        new OgcAxis("E", OgcOrientationType.East),
                        new OgcAxis("N", OgcOrientationType.North) 
                    },
                    new AuthorityTag("EPSG", "27700")
                ),
                new OgcCrsVertical(
                    "Newlyn",
                    new OgcDatum("Ordnance Datum Newlyn", OgcDatumType.VerticalGeoidModelDerived, new AuthorityTag("EPSG", "5101")),
                    new OgcLinearUnit("metre", 1, new AuthorityTag("EPSG", "9001")),
                    new OgcAxis("Up", OgcOrientationType.Up),
                    new AuthorityTag("EPSG", "5701")
                ),
                new AuthorityTag("EPSG", "7405")
            );

            var resultPretty = Pretty.Serialize(input);
            Assert.AreEqual(expectedPretty, resultPretty);
            var resultFlat = Default.Serialize(input);
            Assert.AreEqual(expectedFlat, resultFlat);
        }

        [Test]
        public void LocalCrsTest() {
            var expectedPretty = String.Join(Environment.NewLine, new[]{
                "LOCAL_CS[\"Astra Minas Grid\",",
                "\tLOCAL_DATUM[\"Astra Minas\",10000,AUTHORITY[\"EPSG\",\"9300\"]],",
                "\tUNIT[\"metre\",1],",
                "\tAXIS[\"X\",NORTH],",
                "\tAXIS[\"Y\",WEST],",
                "\tAUTHORITY[\"EPSG\",\"5800\"]]"
            });
            var expectedFlat = expectedPretty
                .Replace("\t", "")
                .Replace(Environment.NewLine, "");

            var input = new OgcCrsLocal(
                "Astra Minas Grid",
                new MockDatum {
                    Name = "Astra Minas",
                    Type = "Engineering",
                    Authority = new AuthorityTag("EPSG", "9300")
                },
                new OgcLinearUnit("metre", 1.0),
                new[]{
                    new MockAxis{ Name="X", Orientation = "north"},
                    new MockAxis{ Name="Y", Orientation = "west"}
                },
                new AuthorityTag("EPSG", "5800")
            );

            Assert.AreEqual(expectedPretty, Pretty.Serialize(input));
            Assert.AreEqual(expectedFlat, Default.Serialize(input));
        }

        [Test]
        public void SerializeFittedTest() {
            var expectedPretty = String.Join(Environment.NewLine, new[] {
                "FITTED_CS[\"Test Fitted\",",
                "\tPARAM_MT[\"Abc\",",
                "\t\tPARAMETER[\"def\",\"blah\"]],",
                "\tGEOCCS[\"aaaa\",",
                "\t\tDATUM[\"?\",",
                "\t\t\tSPHEROID[\"a ball\",123456,278]],",
                "\t\tPRIMEM[\"most important place in the word\",99999],",
                "\t\tUNIT[\"a stick\",1.123],",
                "\t\tAXIS[\"a\",UP]]]"
            });
            var expectedFlat = expectedPretty
                .Replace("\t", "")
                .Replace(Environment.NewLine, "");

            var input = new OgcCrsFitted(
                "Test Fitted",
                new CoordinateOperationInfo(
                    "test",
                    new[] { new NamedParameter<string>("Def", "blah") },
                    new OgcCoordinateOperationMethodInfo("Abc")
                ),
                new OgcCrsGeocentric(
                    "aaaa",
                    new OgcDatumHorizontal(
                        "?",
                        new OgcSpheroid(
                            new SpheroidEquatorialInvF(123456, 278),
                            "a ball",
                            OgcLinearUnit.DefaultMeter
                        ),
                        new OgcPrimeMeridian("most important place in the word", 99999),
                        null
                    ),
                    new OgcLinearUnit("a stick", 1.123),
                    new IAxis[] { new MockAxis { Name = "a", Orientation = "up" } },
                    null
                )
            );

            Assert.AreEqual(expectedPretty, Pretty.Serialize(input));
            Assert.AreEqual(expectedFlat, Default.Serialize(input));
        }

        [Test]
        public void SerializePassThrough() {
            var input = new OgcPassThroughCoordinateOperationInfo(
                new CoordinateOperationInfo(
                    "test",
                    new[] { new NamedParameter<double>("thing", 2) }
                ),
                1
            );

            var expectedPretty = String.Join(Environment.NewLine, new[] {
                "PASSTHROUGH_MT[1,PARAM_MT[\"test\",",
                "\tPARAMETER[\"thing\",2]]]"
            });
            var expectedFlat = expectedPretty
                .Replace("\t", "")
                .Replace(Environment.NewLine, "");

            Assert.AreEqual(expectedPretty, Pretty.Serialize(input));
            Assert.AreEqual(expectedFlat, Default.Serialize(input));
        }

    }
}
