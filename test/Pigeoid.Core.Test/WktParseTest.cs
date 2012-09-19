using System;
using System.Linq;
using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.Ogc;

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

			var coordinateOperationInfo = Default.Parse(input) as ICoordinateOperationInfo;

			Assert.IsNotNull(coordinateOperationInfo);
			Assert.AreEqual("abc", Default.Options.GetEntityName(coordinateOperationInfo));
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

			var coordinateOperationInfo = Default.Parse(input) as ICoordinateOperationInfo;

			Assert.IsNotNull(coordinateOperationInfo);
			Assert.AreEqual("abc", Default.Options.GetEntityName(coordinateOperationInfo));
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
				(result.Steps.Skip(1).First())
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

			var core = result.GetInverse();
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
			var result = Default.Parse(input) as ICoordinateOperationInfo;
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

			var result = Default.Parse(input) as IUom;

			Assert.IsNotNull(result);
			Assert.AreEqual("metre", result.Name);

			if (result is OgcUnitBase) {
				Assert.AreEqual(1, ((OgcUnitBase)result).Factor);
			}
			
			Assert.IsNotNull(result as IAuthorityBoundEntity);
			Assert.AreEqual("EPSG", (result as IAuthorityBoundEntity).Authority.Name);
			Assert.AreEqual("9001", (result as IAuthorityBoundEntity).Authority.Code);
		}

		[Test]
		public void ParseOgcWktSample() {
			var input = String.Join(Environment.NewLine,new[]{
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

			Assert.Inconclusive();
		}

	}
}
