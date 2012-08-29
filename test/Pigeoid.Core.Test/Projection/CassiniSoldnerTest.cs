using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
	[TestFixture]
	public class CassiniSoldnerTest
	{

		[Test]
		public void EpsgExample134Test()
		{
			var projection = new CassiniSoldner(
				new GeographicCoordinate(0.182241463, -1.070468608), 
				new Vector2(430000.00, 325000.00),
				new SpheroidEquatorialInvF(31706587.88, 294.2606764)
			);

			var projected = projection.TransformValue(new GeographicCoordinate(0.17453293, -1.08210414));

			Assert.AreEqual(66644.94, projected.X, 0.2);
			Assert.AreEqual(82536.22, projected.Y, 0.2);
		}

		[Test]
		public void EpsgExample134InverseTest()
		{
			var projection = new CassiniSoldner(
				new GeographicCoordinate(0.182241463, -1.070468608),
				new Vector2(430000.00, 325000.00),
				new SpheroidEquatorialInvF(31706587.88, 294.2606764)
			);

			var unProjected = projection.GetInverse().TransformValue(new Point2(66644.94, 82536.22));

			Assert.AreEqual(0.17453293, unProjected.Latitude, 0.00000004);
			Assert.AreEqual(-1.08210414, unProjected.Longitude, 0.000000004);
		}

	}
}
