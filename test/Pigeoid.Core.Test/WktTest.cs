using System;
using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.Ogc;

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
		public void AuthorityTest(string input, string expectedAuthority, string expectedCode) {
			var authorityTag = Default.Parse(input) as IAuthorityTag;
			Assert.IsNotNull(authorityTag);
			Assert.AreEqual(expectedAuthority, authorityTag.Name);
			Assert.AreEqual(expectedCode, authorityTag.Code);
		}

		[Test]
		[TestCase("PARAMETER[\"Abc\",56]","Abc",56.0)]
		[TestCase(" pArAmEtEr\n[ \"D_e_F\"\t,+45.6e-2\n]\n","D e F", 45.6e-2)]
		[TestCase(" pArAmEtEr\n[ \"ghi\"\t,\"a\nb\"]\n", "ghi", "a\nb")]
		public void NamedParameterTest(string input, string expecgtedName, object expectedValue) {
			var result = Default.Parse(input) as INamedParameter;
			Assert.IsNotNull(result);
			Assert.AreEqual(expecgtedName, result.Name);
			Assert.AreEqual(expectedValue, result.Value);
		}

	}
}
