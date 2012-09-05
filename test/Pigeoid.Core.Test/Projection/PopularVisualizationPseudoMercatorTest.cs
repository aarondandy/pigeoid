using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
	[TestFixture]
	public class PopularVisualizationPseudoMercatorTest
	{

		[Test]
		public void EpsgExample_1_3_3_2_Test()
		{
			var projection = new PopularVisualizationPseudoMercator(
				new GeographicCoordinate(0, 0),
 				new Vector2(0, 0),
 				new SpheroidEquatorialPolar(6378137, 298.2572236)
			);
			var input = new GeographicCoordinate(0.425542460, -1.751147016);
			var expected = new Point2(-11169055.58, 2800000.00);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.005);
			Assert.AreEqual(expected.Y, result.Y, 0.006);
		}

		[Test]
		public void EpsgExample_1_3_3_2_InverseTest()
		{
			var projection = new PopularVisualizationPseudoMercator(
				new GeographicCoordinate(0, 0),
				new Vector2(0, 0),
				new SpheroidEquatorialPolar(6378137, 298.2572236)
			);
			var expected = new GeographicCoordinate(0.425542460, -1.751147016);
			var input = new Point2(-11169055.58, 2800000.00);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000008);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.0000000008);
		}

	}
}
