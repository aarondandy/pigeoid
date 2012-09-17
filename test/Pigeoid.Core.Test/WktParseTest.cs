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
		public void InverseTransformTest() {

			const string testString = "InverSE_Mt [\tPARAM_MT[\"abc\",PARAMETER[\"def\",123]\n]\n]";

			var result = Default.Parse(testString) as ICoordinateOperationInfo;

			Assert.IsNotNull(result);
			Assert.IsTrue(result.HasInverse);

			var core = result.GetInverse();
			Assert.AreEqual("abc", core.Name);
			Assert.AreEqual(1, core.Parameters.Count());
			Assert.AreEqual(123, core.Parameters.First().Value);

		}

	}
}
