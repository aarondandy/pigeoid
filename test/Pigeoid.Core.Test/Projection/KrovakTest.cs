using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
	[TestFixture]
	public class KrovakTest
	{
		[Test]
		public void Espg_1_3_2_1_Test()
		{
			var projection = new Krovak(
				new GeographicCoordinate(0.863937979, 0.741764932),
				1.370083463,
				0.528627762,
				0.9999,
				Vector2.Zero,
				new SpheroidEquatorialInvF(6377397.155, 299.15281)
			);
			var input = new GeographicCoordinate(0.876312568, 0.602425500); // note: read up on Krovak and the Ferro meridian
			var expected = new Point2(1050538.63, 568991.00);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.01);
			Assert.AreEqual(expected.Y, result.Y, 0.1);
		}

		[Test]
		public void Espg_1_3_2_1_InverseTest()
		{
			var projection = new Krovak(
				new GeographicCoordinate(0.863937979, 0.741764932),
				1.370083463,
				0.528627762,
				0.9999,
				Vector2.Zero,
				new SpheroidEquatorialInvF(6377397.155, 299.15281)
			);
			var expected = new GeographicCoordinate(0.876312568, 0.602425500); // note: read up on Krovak and the Ferro meridian
			var input = new Point2(1050538.63, 568991.00);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000008);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.000000002);
		}

	}
}
