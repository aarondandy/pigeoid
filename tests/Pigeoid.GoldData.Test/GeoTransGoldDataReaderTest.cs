using System.IO;
using NUnit.Framework;
using Vertesaur;

namespace Pigeoid.GoldData.Test
{
	[TestFixture]
	public class GeoTransGoldDataReaderTest
	{

		[Test]
		public void BasicTest()
		{

			var reader = new StringReader(
@"COORDINATES: Lambert Conformal Conic (2 parallel)
DATUM: WGE
# ELLIPSOID: WE
CENTRAL MERIDIAN: 57.53847 #deg
ORIGIN LATITUDE: -20.26430 #deg
STANDARD PARALLEL ONE: -20.52861 #deg
STANDARD PARALLEL TWO: -20.00000 #deg
FALSE EASTING: 0 #m
FALSE NORTHING: 0 #m
# Easting (m), Northing (m)
END OF HEADER

# Mauritius
   392,  29260  #1
  1003,  29260  #2
  3241,  29260  #3
 11991,  29256  #4
 25373,  -3232  #5
 -2361, -29261  #6
-25345, -22422  #7
   392,  29260  #8
");

			var parser = new GeoTransGoldDataReader(reader);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual("Lambert Conformal Conic (2 parallel)", parser["COORDINATES"]);
			Assert.AreEqual("WGE", parser["DATUM"]);
			Assert.AreEqual("57.53847", parser["CENTRAL MERIDIAN"]);
			Assert.AreEqual("-20.26430", parser["ORIGIN LATITUDE"]);
			Assert.AreEqual("-20.52861", parser["STANDARD PARALLEL ONE"]);
			Assert.AreEqual("-20.00000", parser["STANDARD PARALLEL TWO"]);
			Assert.AreEqual("0", parser["FALSE EASTING"]);
			Assert.AreEqual("0", parser["FALSE NORTHING"]);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual(
				new Point2(392, 29260),
				parser.CurrentPoint2D()
			);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual(
				new Point2(1003, 29260),
				parser.CurrentPoint2D()
			);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual(
				new Point2(3241, 29260),
				parser.CurrentPoint2D()
			);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual(
				new Point2(11991, 29256),
				parser.CurrentPoint2D()
			);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual(
				new Point2(25373, -3232),
				parser.CurrentPoint2D()
			);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual(
				new Point2(-2361, -29261),
				parser.CurrentPoint2D()
			);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual(
				new Point2(-25345, -22422),
				parser.CurrentPoint2D()
			);
			Assert.IsTrue(parser.Read());
			Assert.AreEqual(
				new Point2(392, 29260),
				parser.CurrentPoint2D()
			);
			Assert.IsFalse(parser.Read());
		}

	}
}
