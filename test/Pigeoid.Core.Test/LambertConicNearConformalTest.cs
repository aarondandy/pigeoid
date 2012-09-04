using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test
{
	[TestFixture]
	public class LambertConicNearConformalTest
	{

		[Test]
		public void Epsg_1_3_1_5_Test()
		{
			var projection = new LambertConicNearConformal(
				new GeographicCoordinate(0.604756586, 0.651880476),
				0.99962560,
				new Vector2(300000.00, 300000.00),
				new SpheroidEquatorialInvF(6378249.2, 293.46602)
			);
			var input = new GeographicCoordinate(0.654874806, 0.595793792);
			var expected = new Point2(15707.96, 623165.96);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.0004);
			Assert.AreEqual(expected.Y, result.Y, 0.02);
		}

		[Test]
		public void Epsg_1_3_1_5_Inverse_Test()
		{
			var projection = new LambertConicNearConformal(
				new GeographicCoordinate(0.604756586, 0.651880476),
				0.99962560,
				new Vector2(300000.00, 300000.00),
				new SpheroidEquatorialInvF(6378249.2, 293.46602)
			);
			var expected = new GeographicCoordinate(0.654874806, 0.595793792);
			var input = new Point2(15707.96, 623165.96);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000002);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.00000000008);
		}

	}
}
