using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
	[TestFixture]
	public class StereographicTest
	{

		[Test]
		public void EpsgExample_1_3_7_1()
		{
			var projection = new ObliqueStereographic(
				new GeographicCoordinate(0.910296727, 0.094032038),
				0.9999079,
				new Vector2(155000.00, 463000.00),
				new SpheroidEquatorialInvF(6377397.155, 299.15281)
			);
			var input = new GeographicCoordinate(0.925024504, 0.104719755);
			var expected = new Point2(196105.283, 557057.739);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X);
			Assert.AreEqual(expected.Y, result.Y);
		}

		[Test]
		public void EpsgExample_1_3_7_2_A()
		{
			var projection = new PolarStereographicA(
				new GeographicCoordinate(1.570796327, 0),
				0.994,
				new Vector2(2000000.00, 2000000.00),
				new SpheroidEquatorialInvF(6378137, 298.2572236)
			);
			var input = new GeographicCoordinate(1.274090354, 0.767944871);
			var expected = new Point2(3320416.75, 632668.43);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.003);
			Assert.AreEqual(expected.Y, result.Y, 0.002);
		}

		[Test]
		public void EpsgExample_1_3_7_2_A_Inverse()
		{
			var projection = new PolarStereographicA(
				new GeographicCoordinate(1.570796327, 0),
				0.994,
				new Vector2(2000000.00, 2000000.00),
				new SpheroidEquatorialInvF(6378137, 298.2572236)
			);
			var expected = new GeographicCoordinate(1.274090354, 0.767944871);
			var input = new Point2(3320416.75, 632668.43);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000005);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.000000001);
		}

		[Test]
		public void EpsgExample_1_3_7_2_B()
		{
			var projection = new PolarStereographicB(
				new GeographicCoordinate(-1.239183769, 1.221730476),
				new Vector2(6000000.00, 6000000.00),
				new SpheroidEquatorialInvF(6378137, 298.2572236)
			);
			var input = new GeographicCoordinate(-1.308996939, 2.094395102);
			var expected = new Point2(7255380.79, 7053389.56);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.004);
			Assert.AreEqual(expected.Y, result.Y, 0.001);
		}

		[Test]
		public void EpsgExample_1_3_7_2_B_Inverse()
		{
			var projection = new PolarStereographicB(
				new GeographicCoordinate(-1.239183769, 1.221730476),
				new Vector2(6000000.00, 6000000.00),
				new SpheroidEquatorialInvF(6378137, 298.2572236)
			);
			var expected = new GeographicCoordinate(-1.308996939, 2.094395102);
			var input = new Point2(7255380.79, 7053389.56);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000005);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.000000001);
		}

		[Test]
		public void EpsgExample_1_3_7_2_C()
		{
			var projection = new PolarStereographicC(
				new GeographicCoordinate(-1.169370599, 2.443460953),
				new Vector2(300000.00, 200000.00),
				new SpheroidEquatorialInvF(6378388, 297)
			);
			var input = new GeographicCoordinate(-1.162480524, 2.444707118);
			var expected = new Point2(303169.52, 244055.72);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.002);
			Assert.AreEqual(expected.Y, result.Y, 0.00004);
		}

		[Test]
		public void EpsgExample_1_3_7_2_C_Inverse()
		{
			var projection = new PolarStereographicC(
				new GeographicCoordinate(-1.169370599, 2.443460953),
				new Vector2(300000.00, 200000.00),
				new SpheroidEquatorialInvF(6378388, 297)
			);
			var expected = new GeographicCoordinate(-1.162480524, 2.444707118);
			var input = new Point2(303169.52, 244055.72);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.000000000002);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.0000000007);
		}

	}
}
