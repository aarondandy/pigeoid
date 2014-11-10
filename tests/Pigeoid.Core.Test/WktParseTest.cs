using System;
using System.Linq;
using NUnit.Framework;
using Pigeoid.CoordinateOperation;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Ogc;
using Pigeoid.Unit;
using Vertesaur;

namespace Pigeoid.Core.Test
{
    [TestFixture]
    public class WktParseTest
    {

        public readonly WktSerializer Default = new WktSerializer();

        [Test]
        [TestCase("1.9", 1.9)]
        [TestCase(".9", 0.9)]
        [TestCase("2", 2.0)]
        [TestCase("+2", 2.0)]
        [TestCase("+2e10", 20000000000.0)]
        [TestCase("-.9e-1", -0.09)]
        public void ParseDoubleTest(string input, double expected) {
            Assert.AreEqual(expected, Default.Parse(input));
        }

        [Test]
        [TestCase("a")]
        [TestCase("\0")]
        [TestCase("AbC")]
        [TestCase("\t \r\n")]
        public void ParseQuotedStringTest(string text) {
            const char doubleQuote = '\"';
            Assert.AreEqual(text, Default.Parse(doubleQuote + text + doubleQuote));
            Assert.AreEqual(text, Default.Parse(doubleQuote + text));
        }

        [Test]
        public void ParseEmptyStringTest() {
            Assert.AreEqual(String.Empty, Default.Parse("\"\""));
            Assert.AreEqual(null, Default.Parse("\""));
        }

        [Test]
        [TestCase("AUTHORITY[\"EPSG\",\"1234\"]", "EPSG", "1234")]
        [TestCase(" aUtHoRiTy ( \"AbC\"\t,\n\"\" ]  ", "AbC", "")]
        public void ParseAuthorityTest(string input, string expectedAuthority, string expectedCode) {
            var authorityTag = Default.Parse(input) as IAuthorityTag;
            Assert.IsNotNull(authorityTag);
            Assert.AreEqual(expectedAuthority, authorityTag.Name);
            Assert.AreEqual(expectedCode, authorityTag.Code);
        }

        [Test]
        [TestCase("PARAMETER[\"Abc\",56]", "Abc", 56.0)]
        [TestCase(" pArAmEtEr\n[ \"D_e_F\"\t,+45.6e-2\n]\n", "D e F", 45.6e-2)]
        [TestCase(" pArAmEtEr\n[ \"ghi\"\t,\"a\nb\"]\n", "ghi", "a\nb")]
        public void ParseNamedParameterTest(string input, string expecgtedName, object expectedValue) {
            var result = Default.Parse(input) as INamedParameter;
            Assert.IsNotNull(result);
            Assert.AreEqual(expecgtedName, result.Name);
            Assert.AreEqual(expectedValue, result.Value);
        }

        [Test]
        public void ParseParamMathTransformTestA() {
            const string input = "PARAM_MT[\"abc\",PARAMETER[\"ABC\",56],PARAMETER[\"DEF\",+45.6E-2]]";

            var coordinateOperationInfo = Default.Parse(input) as IParameterizedCoordinateOperationInfo;

            Assert.IsNotNull(coordinateOperationInfo);
            Assert.AreEqual("abc", coordinateOperationInfo.Name);
            Assert.AreEqual("ABC", coordinateOperationInfo.Parameters.First().Name);
            Assert.AreEqual(56.0, coordinateOperationInfo.Parameters.First().Value);
            Assert.AreEqual("DEF", coordinateOperationInfo.Parameters.Skip(1).First().Name);
            Assert.AreEqual(.456, coordinateOperationInfo.Parameters.Skip(1).First().Value);
            Assert.AreEqual(2, coordinateOperationInfo.Parameters.Count());
        }

        [Test]
        public void ParseParamMathTransformTestB() {
            const string input = " PARAM_MT [\t\"abc\",pArAmEtEr\n[ \"DeF\"\t,+45.6e2\n]\t]\n";

            var coordinateOperationInfo = Default.Parse(input) as IParameterizedCoordinateOperationInfo;

            Assert.IsNotNull(coordinateOperationInfo);
            Assert.AreEqual("abc", coordinateOperationInfo.Name);
            Assert.AreEqual("DeF", coordinateOperationInfo.Parameters.First().Name);
            Assert.AreEqual(4560, coordinateOperationInfo.Parameters.First().Value);
            Assert.AreEqual(1, coordinateOperationInfo.Parameters.Count());
        }

        [Test]
        public void ParseConcatMathTransformTest() {

            const string input =
                " CONCAT_MT    ["
                + "PARAM_MT[\"abc\",PARAMETER[\"ABC\",56],PARAMETER[\"DEF\",+45.6E-2]]"
                + "\n,\t"
                + " PARAM_MT [\t\"def\",pArAmEtEr\n[ \"DeF\"\t,+45.6e2\n]\t]\n"
                + "\t]";

            var result = Default.Parse(input) as IConcatenatedCoordinateOperationInfo;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Steps.Count());
            Assert.AreEqual("abc", result.Steps.First().Name);
            Assert.AreEqual(
                4560,
                (result.Steps.Skip(1).First() as IParameterizedCoordinateOperationInfo)
                .Parameters
                .First()
                .Value
            );

        }

        [Test]
        public void ParseInverseTransformTest() {
            const string testString = "InverSE_Mt [\tPARAM_MT[\"abc\",PARAMETER[\"def\",123]\n]\n]";

            var result = Default.Parse(testString) as ICoordinateOperationInfo;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasInverse);
            Assert.IsTrue(result.IsInverseOfDefinition);

            var core = result.GetInverse() as IParameterizedCoordinateOperationInfo;
            Assert.IsNotNull(core);
            Assert.AreEqual("abc", core.Name);
            Assert.AreEqual(1, core.Parameters.Count());
            Assert.AreEqual(123, core.Parameters.First().Value);
        }

        [Test]
        public void ParsePassThroughTransformTest() {
            const string testString = "PASSTHROUGH_MT[2,PASSTHROUGH_MT[3,PARAM_MT[\"dookie\"]]]";

            var passThrough = Default.Parse(testString) as IPassThroughCoordinateOperationInfo;
            Assert.IsNotNull(passThrough);
            Assert.AreEqual(2, passThrough.FirstAffectedOrdinate);

            var passThrough2 = passThrough.Steps.First() as IPassThroughCoordinateOperationInfo;
            Assert.IsNotNull(passThrough2);
            Assert.AreEqual(3, passThrough2.FirstAffectedOrdinate);

            var coordinateOperation = passThrough2.Steps.First();
            Assert.IsNotNull(coordinateOperation);
            Assert.AreEqual("dookie", coordinateOperation.Name);
        }

        [Test]
        public void ParseMercatorSample() {
            const string input =
@"PARAM_MT[""Mercator_2SP"",
    PARAMETER[""semi_major"",6370997.0],
    PARAMETER[""semi_minor"",6370997.0], 
    PARAMETER[""central_meridian"",180.0], 
    PARAMETER[""false_easting"",-500000.0], 
    PARAMETER[""false_northing"",-1000000.0], 
    PARAMETER[""standard parallel 1"",60.0]
]";
            var result = Default.Parse(input) as IParameterizedCoordinateOperationInfo;
            Assert.IsNotNull(result);
            var operationParameters = result.Parameters.ToArray();

            Assert.AreEqual(6, operationParameters.Length);
            Assert.AreEqual("semi major", operationParameters[0].Name);
            Assert.AreEqual(6370997.0, operationParameters[0].Value);
            Assert.AreEqual("semi minor", operationParameters[1].Name);
            Assert.AreEqual(6370997.0, operationParameters[1].Value);
            Assert.AreEqual("central meridian", operationParameters[2].Name);
            Assert.AreEqual(180.0, operationParameters[2].Value);
            Assert.AreEqual("false easting", operationParameters[3].Name);
            Assert.AreEqual(-500000.0, operationParameters[3].Value);
            Assert.AreEqual("false northing", operationParameters[4].Name);
            Assert.AreEqual(-1000000.0, operationParameters[4].Value);
            Assert.AreEqual("standard parallel 1", operationParameters[5].Name);
            Assert.AreEqual(60.0, operationParameters[5].Value);
        }

        [Test]
        public void ParseSpheroidTest() {
            const string input = @"SPHEROID[
""Airy 1830"",6377563.396,299.3249646,
AUTHORITY[""EPSG"",""7001""]
]";
            var result = Default.Parse(input) as ISpheroidInfo;
            Assert.IsNotNull(result);
            Assert.AreEqual("Airy 1830", result.Name);
            Assert.AreEqual(6377563.396, result.A);
            Assert.AreEqual(299.3249646, result.InvF);
            Assert.IsNotNull(result.Authority);
            Assert.AreEqual("EPSG", result.Authority.Name);
            Assert.AreEqual("7001", result.Authority.Code);
        }

        [Test]
        public void ParsePrimeMeridianTest() {
            const string input = @"PRIMEM[""Greenwich"",123,AUTHORITY[""EPSG"",""8901""]]";
            var result = Default.Parse(input) as IPrimeMeridianInfo;
            Assert.IsNotNull(result);
            Assert.AreEqual("Greenwich", result.Name);
            Assert.AreEqual(123, result.Longitude);
            Assert.AreEqual("EPSG", result.Authority.Name);
            Assert.AreEqual("8901", result.Authority.Code);
        }

        [Test]
        public void ParseUnitTest() {
            const string input = @"UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]]";

            var result = Default.Parse(input) as IUnit;

            Assert.IsNotNull(result);
            Assert.AreEqual("metre", result.Name);

            var ogcUnitBase = result as OgcUnitBase;
            if (ogcUnitBase != null) {
                Assert.AreEqual(1, ogcUnitBase.Factor);
            }
            else {
                Assert.Inconclusive();
            }

            Assert.IsNotNull(result as IAuthorityBoundEntity);
            Assert.AreEqual("EPSG", (result as IAuthorityBoundEntity).Authority.Name);
            Assert.AreEqual("9001", (result as IAuthorityBoundEntity).Authority.Code);
        }

        [Test]
        public void FittedTest() {
            var input = String.Join(Environment.NewLine, new[] {
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

            var result = Default.Parse(input);
            Assert.IsNotNull(result);
            var fitted = result as ICrsFitted;
            Assert.IsNotNull(fitted);
            Assert.AreEqual("Test Fitted", fitted.Name);
            var coordinateOperation = fitted.ToBaseOperation as IParameterizedCoordinateOperationInfo;
            Assert.IsNotNull(coordinateOperation);
            Assert.AreEqual("Abc", coordinateOperation.Name);
            Assert.AreEqual(coordinateOperation.IsInverseOfDefinition, false);
            Assert.AreEqual(coordinateOperation.HasInverse, true);
            Assert.AreEqual(1, coordinateOperation.Parameters.Count());
            Assert.AreEqual("def", coordinateOperation.Parameters.First().Name);
            Assert.AreEqual("blah", coordinateOperation.Parameters.First().Value);
            Assert.AreEqual("Abc", coordinateOperation.Method.Name);
            var baseCrs = fitted.BaseCrs as ICrsGeodetic;
            Assert.IsNotNull(baseCrs);
            Assert.AreEqual("aaaa", baseCrs.Name);
            var datum = baseCrs.Datum;
            Assert.IsNotNull(datum);
            Assert.AreEqual("?", datum.Name);
            var spheroid = datum.Spheroid;
            Assert.IsNotNull(spheroid);
            Assert.AreEqual(123456, spheroid.A);
            Assert.AreEqual(278, spheroid.InvF);
            Assert.AreEqual("a ball", spheroid.Name);
            var meridian = datum.PrimeMeridian;
            Assert.IsNotNull(meridian);
            Assert.AreEqual("most important place in the word", meridian.Name);
            Assert.AreEqual(99999, meridian.Longitude);
            var unit = baseCrs.Unit;
            Assert.IsNotNull(unit);
            Assert.AreEqual("a stick", unit.Name);
            Assert.AreEqual(1.123, (SimpleUnitConversionGenerator.FindConversion(unit, OgcLinearUnit.DefaultMeter) as IUnitScalarConversion<double>).Factor);
            var axis = baseCrs.Axes.FirstOrDefault();
            Assert.IsNotNull(axis);
            Assert.AreEqual("a", axis.Name);
            Assert.AreEqual("Up", axis.Orientation);
        }

        [Test]
        public void LocalCrsTest() {
            var input = String.Join(Environment.NewLine, new[]{
				"LOCAL_CS[\"Astra Minas Grid\",",
				"\tLOCAL_DATUM[\"Astra Minas\",10000,AUTHORITY[\"EPSG\",\"9300\"]],",
				"\tUNIT[\"metre\",1],",
				"\tAXIS[\"X\",NORTH],",
				"\tAXIS[\"Y\",WEST],",
				"\tAUTHORITY[\"EPSG\",\"5800\"]]"
			});

            var result = Default.Parse(input) as ICrsLocal;
            Assert.IsNotNull(result);
            Assert.AreEqual("Astra Minas Grid", result.Name);
            Assert.AreEqual(new AuthorityTag("EPSG", "5800"), result.Authority);
            var datum = result.Datum;
            Assert.IsNotNull(datum);
            Assert.AreEqual("Astra Minas", datum.Name);
            Assert.AreEqual("Local", datum.Type);
            Assert.AreEqual(new AuthorityTag("EPSG", "9300"), datum.Authority);
            Assert.IsNotNull(result.Unit);
            Assert.AreEqual("metre", result.Unit.Name);
            Assert.AreEqual(1, (SimpleUnitConversionGenerator.FindConversion(result.Unit, OgcLinearUnit.DefaultMeter) as IUnitScalarConversion<double>).Factor);
            Assert.IsNotNull(result.Axes);
            Assert.AreEqual(2, result.Axes.Count);
            Assert.AreEqual("X", result.Axes[0].Name);
            Assert.AreEqual("North", result.Axes[0].Orientation);
            Assert.AreEqual("Y", result.Axes[1].Name);
            Assert.AreEqual("West", result.Axes[1].Orientation);
        }

        [Test]
        public void ParseOgcWktSample() {
            var input = String.Join(Environment.NewLine, new[]{
				"COMPD_CS[\"OSGB36 / British National Grid + ODN\",",
				"\tPROJCS[\"OSGB 1936 / British National Grid\",",
				"\t\tGEOGCS[\"OSGB 1936\",",
				"\t\t\tDATUM[\"OSGB_1936\",",
				"\t\t\t\tSPHEROID[\"Airy 1830\",6377563.396,299.3249646,AUTHORITY[\"EPSG\",\"7001\"]],",
				"\t\t\t\tTOWGS84[375,-111,431,0,0,0,0],",
				"\t\t\t\tAUTHORITY[\"EPSG\",\"6277\"]",
				"\t\t\t],",
				"\t\t\tPRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],",
				"\t\t\tUNIT[\"DMSH\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9108\"]],",
				"\t\t\tAXIS[\"Lat\",NORTH],",
				"\t\t\tAXIS[\"Long\",EAST],",
				"\t\t\tAUTHORITY[\"EPSG\",\"4277\"]",
				"\t\t],",
				"\t\tPROJECTION[\"Transverse_Mercator\"],",
				"\t\tPARAMETER[\"latitude_of_origin\",49],",
				"\t\tPARAMETER[\"central_meridian\",-2],",
				"\t\tPARAMETER[\"scale_factor\",0.999601272],",
				"\t\tPARAMETER[\"false_easting\",400000],",
				"\t\tPARAMETER[\"false_northing\",-100000],",
				"\t\tUNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],",
				"\t\tAXIS[\"E\",EAST],",
				"\t\tAXIS[\"N\",NORTH],",
				"\t\tAUTHORITY[\"EPSG\",\"27700\"]",
				"\t],",
				"\tVERT_CS[\"Newlyn\",",
				"\t\tVERT_DATUM[\"Ordnance Datum Newlyn\",2005,AUTHORITY[\"EPSG\",\"5101\"]],",
				"\t\tUNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],",
				"\t\tAXIS[\"Up\",UP],",
				"\t\tAUTHORITY[\"EPSG\",\"5701\"]",
				"\t],",
				"\tAUTHORITY[\"EPSG\",\"7405\"]",
				"]"
			});

            var result = Default.Parse(input);
            Assert.IsNotNull(result);
            var compound = result as ICrsCompound;
            Assert.IsNotNull(compound);
            Assert.AreEqual("OSGB36 / British National Grid + ODN", compound.Name);
            Assert.AreEqual(new AuthorityTag("EPSG", "7405"), compound.Authority);

            var projected = compound.Head as ICrsProjected;
            Assert.IsNotNull(projected);
            Assert.AreEqual("OSGB 1936 / British National Grid", projected.Name);
            Assert.AreEqual(new AuthorityTag("EPSG", "27700"), projected.Authority);
            Assert.IsNotNull(projected.Unit);
            Assert.AreEqual("metre", projected.Unit.Name);
            Assert.That(projected.Unit is IAuthorityBoundEntity);
            Assert.AreEqual(new AuthorityTag("EPSG", "9001"), (projected.Unit as IAuthorityBoundEntity).Authority);
            var unit = projected.Unit as OgcUnitBase;
            Assert.IsNotNull(unit);
            Assert.AreEqual(1, unit.Factor);
            var operation = projected.Projection as IParameterizedCoordinateOperationInfo;
            Assert.IsNotNull(operation);
            Assert.AreEqual("Transverse Mercator", operation.Name);
            Assert.IsNotNull(operation.Parameters);
            var opParams = operation.Parameters.ToList();
            Assert.AreEqual(5, opParams.Count);
            Assert.AreEqual("latitude of origin", opParams[0].Name);
            Assert.AreEqual(49.0, opParams[0].Value);
            Assert.AreEqual("central meridian", opParams[1].Name);
            Assert.AreEqual(-2.0, opParams[1].Value);
            Assert.AreEqual("scale factor", opParams[2].Name);
            Assert.AreEqual(0.999601272, opParams[2].Value);
            Assert.AreEqual("false easting", opParams[3].Name);
            Assert.AreEqual(400000.0, opParams[3].Value);
            Assert.AreEqual("false northing", opParams[4].Name);
            Assert.AreEqual(-100000.0, opParams[4].Value);
            Assert.IsNotNull(projected.Axes);
            Assert.AreEqual(2, projected.Axes.Count);
            Assert.AreEqual("E", projected.Axes[0].Name);
            Assert.AreEqual("East", projected.Axes[0].Orientation);
            Assert.AreEqual("N", projected.Axes[1].Name);
            Assert.AreEqual("North", projected.Axes[1].Orientation);

            var geographic = projected.BaseCrs;
            Assert.IsNotNull(geographic);
            Assert.AreEqual("OSGB 1936", geographic.Name);
            Assert.IsNotNull(geographic.Unit);
            Assert.AreEqual("DMSH", geographic.Unit.Name);
            Assert.That(geographic.Unit is IAuthorityBoundEntity);
            Assert.AreEqual(new AuthorityTag("EPSG", "9108"), (geographic.Unit as IAuthorityBoundEntity).Authority);
            unit = geographic.Unit as OgcUnitBase;
            Assert.IsNotNull(unit);
            Assert.AreEqual(Math.PI / 180.0, unit.Factor, 0.000000000001);
            Assert.IsNotNull(geographic.Axes);
            Assert.AreEqual(2, geographic.Axes.Count);
            Assert.AreEqual("Lat", geographic.Axes[0].Name);
            Assert.AreEqual("North", geographic.Axes[0].Orientation);
            Assert.AreEqual("Long", geographic.Axes[1].Name);
            Assert.AreEqual("East", geographic.Axes[1].Orientation);

            var datum = geographic.Datum;
            Assert.IsNotNull(datum);
            Assert.AreEqual("OSGB_1936", datum.Name);
            Assert.IsNotNull(datum.Spheroid);
            Assert.AreEqual(6377563.396, datum.Spheroid.A);
            Assert.AreEqual(299.3249646, datum.Spheroid.InvF);
            Assert.AreEqual("Airy 1830", datum.Spheroid.Name);
            Assert.AreEqual(new AuthorityTag("EPSG", "7001"), datum.Spheroid.Authority);
            Assert.IsTrue(datum.IsTransformableToWgs84);
            Assert.AreEqual(
                new Helmert7Transformation(
                    new Vector3(375, -111, 431),
                    Vector3.Zero,
                    0
                ),
                datum.BasicWgs84Transformation
            );
            Assert.AreEqual(new AuthorityTag("EPSG", "6277"), datum.Authority);
            Assert.IsNotNull(datum.PrimeMeridian);
            Assert.AreEqual("Greenwich", datum.PrimeMeridian.Name);
            Assert.AreEqual(0, datum.PrimeMeridian.Longitude);
            Assert.IsNotNull(datum.PrimeMeridian.Unit);
            var angularUnit = datum.PrimeMeridian.Unit as OgcAngularUnit;
            Assert.IsNotNull(angularUnit);
            Assert.AreEqual(Math.PI / 180.0, angularUnit.Factor, 0.00000000001);
            Assert.AreEqual(new AuthorityTag("EPSG", "8901"), datum.PrimeMeridian.Authority);

            var vertical = compound.Tail as ICrsVertical;
            Assert.IsNotNull(vertical);
            Assert.AreEqual("Newlyn", vertical.Name);
            Assert.IsNotNull(vertical.Unit);
            Assert.AreEqual("metre", vertical.Unit.Name);
            Assert.That(vertical.Unit is IAuthorityBoundEntity);
            Assert.AreEqual(new AuthorityTag("EPSG", "9001"), (vertical.Unit as IAuthorityBoundEntity).Authority);
            var linearUnit = vertical.Unit as OgcLinearUnit;
            Assert.IsNotNull(linearUnit);
            Assert.AreEqual(1, linearUnit.Factor);
            Assert.IsNotNull(vertical.Axis);
            Assert.AreEqual("Up", vertical.Axis.Name);
            Assert.AreEqual("Up", vertical.Axis.Orientation);
            Assert.AreEqual(new AuthorityTag("EPSG", "5701"), vertical.Authority);

            var verticalDatum = vertical.Datum;
            Assert.IsNotNull(verticalDatum);
            Assert.AreEqual("Ordnance Datum Newlyn", verticalDatum.Name);
            Assert.AreEqual(OgcDatumType.VerticalGeoidModelDerived.ToString(), verticalDatum.Type);
            Assert.AreEqual(new AuthorityTag("EPSG", "5101"), verticalDatum.Authority);
        }

    }
}
