using System;
using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
	[TestFixture]
	public class ObliqueMercatorTest
	{
 
		[Test]
		public void Epsg_1_3_6_1_Test()
		{
			throw new NotImplementedException("redo...");
			var projection = new HotineObliqueMercator.VariantA(
				new GeographicCoordinate(0.069813170, 2.007128640),
				0.930536611,
				0.927295218,
				0.99984,
				new Vector2(590476.87, 442857.65),
				new SpheroidEquatorialInvF(6377298.556, 300.8017)
			);
			var input = new GeographicCoordinate(0.094025313, 2.021187362);
			var expected = new Point2(679245.73, 596562.78);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X);
			Assert.AreEqual(expected.Y, result.Y);
		}

		[Test]
		public void Epsg_1_3_6_2_Test()
		{
			var projection = new LabordeObliqueMercator(
				new GeographicCoordinate(-0.329867229, 0.810482544),
 				0.329867229,
				0.9995,
				new SpheroidEquatorialInvF(6378388, 297),
 				new Vector2(400000, 800000)
			);
			var input = new GeographicCoordinate(-0.282565315, 0.735138668);
			var expected = new Point2(188333.848, 1098841.091);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X);
			Assert.AreEqual(expected.Y, result.Y);
		}


	}
}
