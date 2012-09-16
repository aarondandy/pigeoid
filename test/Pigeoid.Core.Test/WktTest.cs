using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.Ogc;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Core.Test
{
	[TestFixture]
	public class WktTest
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
		[TestCase("PARAMETER[\"Abc\",56]","Abc",56.0)]
		[TestCase(" pArAmEtEr\n[ \"D_e_F\"\t,+45.6e-2\n]\n","D e F", 45.6e-2)]
		[TestCase(" pArAmEtEr\n[ \"ghi\"\t,\"a\nb\"]\n", "ghi", "a\nb")]
		public void ParseNamedParameterTest(string input, string expecgtedName, object expectedValue) {
			var result = Default.Parse(input) as INamedParameter;
			Assert.IsNotNull(result);
			Assert.AreEqual(expecgtedName, result.Name);
			Assert.AreEqual(expectedValue, result.Value);
		}

		[Test]
		public void ParseParamMathTransformTestA()
		{
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
		public void ParseParamMathTransformTestB()
		{
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
		public void SerializePrettyConcatMathTransformTest() {
			var input = new ConcatenatedCoordinateOperationInfo(
				new ICoordinateOperationInfo[] {
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
						"Ellipsoid To Geocentric",
						new INamedParameter[]{
							new NamedParameter<double>("semi major", 6378137),
							new NamedParameter<double>("semi minor", 6356752.31414035)
						}
					),
					new CoordinateOperationInfo(
						"Ellipsoid_To_Geocentric",
						new INamedParameter[]{
							new NamedParameter<double>("semi_major", 6378206.4),
							new NamedParameter<double>("semi_minor", 6356583.8)
						}
					), 
				}
			);
			var expected = String.Format(
				"CONCAT_MT[{0}" +
				"\tPARAM_MT[\"Helmert_7_Parameter_Transformation\",{0}" +
				"\t\tPARAMETER[\"dx\",1],{0}" +
				"\t\tPARAMETER[\"dy\",2],{0}" +
				"\t\tPARAMETER[\"dz\",3],{0}" +
				"\t\tPARAMETER[\"rx\",4],{0}" +
				"\t\tPARAMETER[\"ry\",5],{0}" +
				"\t\tPARAMETER[\"rz\",6],{0}" +
				"\t\tPARAMETER[\"m\",7]{0}" +
				"\t],{0}" +
				"\tPARAM_MT[\"Ellipsoid_To_Geocentric\",{0}" +
				"\t\tPARAMETER[\"semi_major\",6378137],{0}" +
				"\t\tPARAMETER[\"semi_minor\",6356752.31414035]{0}" +
				"\t],{0}" +
				"\tINVERSE_MT[{0}" +
				"\t\tPARAM_MT[\"Ellipsoid_To_Geocentric\",{0}" +
				"\t\t\tPARAMETER[\"semi_major\",6378206.4],{0}" +
				"\t\t\tPARAMETER[\"semi_minor\",6356583.8]{0}" +
				"\t\t]{0}" +
				"\t]{0}" +
				"]",
				Environment.NewLine
			);

			Assert.Inconclusive();

			/*
			var aDefault = aPretty.Replace(Environment.NewLine, "").Replace("\t", "");
			Assert.AreEqual(aDefault, Default.Serialize(a));
			Assert.AreEqual(aPretty, Pretty.Serialize(a));*/
		}

	}
}
