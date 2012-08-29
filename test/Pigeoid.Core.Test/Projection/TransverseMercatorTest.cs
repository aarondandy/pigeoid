using System;
using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
	[TestFixture]
	public class TransverseMercatorTest
	{

		[Test]
		public void EpsgExample1351Test()
		{
			var projection = new TransverseMercator(
				new GeographicCoordinate(0.85521133, -0.03490659), 
				new Vector2(400000.00, -100000.00),
				0.9996012717,
				new SpheroidEquatorialInvF(6377563.396, 299.32496)
			);

			var projected = projection.TransformValue(new GeographicCoordinate(0.88139127, 0.00872665));

			Assert.AreEqual(577274.99, projected.X, 0.08);
			Assert.AreEqual(69740.50, projected.Y, 0.005);
		}

		[Test]
		public void EpsgExample1351InverseTest()
		{
			var projection = new TransverseMercator(
				new GeographicCoordinate(0.85521133, -0.03490659),
				new Vector2(400000.00, -100000.00),
				0.9996012717,
				new SpheroidEquatorialInvF(6377563.396, 299.32496)
			);

			var unProjected = projection.GetInverse().TransformValue(new Point2(577274.99, 69740.50));

			Assert.AreEqual(0.88139127, unProjected.Latitude, 0.0000000008);
			Assert.AreEqual(0.00872665, unProjected.Longitude, 0.000000008);
		}

		[Test]
		public void OsgbTest()
		{
			var projection = new TransverseMercator(
				new GeographicCoordinate(49 * Math.PI / 180, -2 * Math.PI / 180), 
				new Vector2(400000, -100000),
				0.9996012717,
				new SpheroidEquatorialInvF(6377563.396, 299.32496)
			);
			var a = new Point2(651409.903, 313177.270);
			var b = new GeographicCoordinate(52.6576 * Math.PI / 180, 1.7179 * Math.PI / 180);

			var projected = projection.TransformValue(b);

			Assert.AreEqual(a.X, projected.X, 2);
			Assert.AreEqual(a.Y, projected.Y, 4);
		}

		[Test]
		public void OsgbInverseTest()
		{
			var projection = new TransverseMercator(
				new GeographicCoordinate(49 * Math.PI / 180, -2 * Math.PI / 180),
				new Vector2(400000, -100000),
				0.9996012717,
				new SpheroidEquatorialInvF(6377563.396, 299.32496)
			);
			var a = new Point2(651409.903, 313177.270);
			var b = new GeographicCoordinate(52.6576 * Math.PI / 180, 1.7179 * Math.PI / 180);

			var unProjected = projection.GetInverse().TransformValue(a);

			Assert.AreEqual(b.Latitude, unProjected.Latitude, 0.0000006);
			Assert.AreEqual(b.Longitude, unProjected.Longitude, 0.0000004);
		}

	}
}
