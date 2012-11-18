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
			var input = new GeographicCoordinate(0.876312568, 0.294084);
			var expected = new Point2(1050538.63, 568991.00);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.007);
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
			var expected = new GeographicCoordinate(0.876312568, 0.294084);
			var input = new Point2(1050538.63, 568991.00);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000008);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.000001);
		}

		[Test]
		public void Espg_1_3_2_2_Test()
		{
			var projection = new KrovakNorth(
				new GeographicCoordinate(0.863937979, 0.741764932),
				1.370083463,
				0.528627762,
				0.9999,
				Vector2.Zero,
				new SpheroidEquatorialInvF(6377397.155, 299.15281)
			);
			var input = new GeographicCoordinate(0.876312568, 0.294084);
			var expected = new Point2(-568991.00 , - 1050538.63);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.1);
			Assert.AreEqual(expected.Y, result.Y, 0.1);
		}

		[Test]
		public void Espg_1_3_2_2_InverseTest()
		{
			var projection = new KrovakNorth(
				new GeographicCoordinate(0.863937979, 0.741764932),
				1.370083463,
				0.528627762,
				0.9999,
				Vector2.Zero,
				new SpheroidEquatorialInvF(6377397.155, 299.15281)
			);
			var expected = new GeographicCoordinate(0.876312568, 0.294084);
			var input = new Point2(-568991.00, -1050538.63);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000008);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.000001);
		}

		[Test]
		public void Espg_1_3_2_3_Test()
		{
			var projection = new KrovakModified(
				new GeographicCoordinate(0.863937979, 0.741764932),
				1.370083463,
				0.528627762,
				0.9999,
				new Vector2(5000000, 5000000),
				new SpheroidEquatorialInvF(6377397.155, 299.15281),
				new Point2(1089000, 654000),
				new[] { 2.946529277E-02, 2.515965696E-02, 1.193845912E-07, -4.668270147E-07, 9.233980362E-12, 1.523735715E-12, 1.696780024E-18, 4.408314235E-18, -8.331083518E-24, -3.689471323E-24}
			);
			var input = new GeographicCoordinate(0.876312568, 0.294084);
			var expected = new Point2(6050538.71, 5568990.91);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.004);
			Assert.AreEqual(expected.Y, result.Y, 0.01);
		}

		[Test]
		public void Espg_1_3_2_3_InverseTest()
		{
			var projection = new KrovakModified(
				new GeographicCoordinate(0.863937979, 0.741764932),
				1.370083463,
				0.528627762,
				0.9999,
				new Vector2(5000000, 5000000),
				new SpheroidEquatorialInvF(6377397.155, 299.15281),
				new Point2(1089000, 654000),
				new[] { 2.946529277E-02, 2.515965696E-02, 1.193845912E-07, -4.668270147E-07, 9.233980362E-12, 1.523735715E-12, 1.696780024E-18, 4.408314235E-18, -8.331083518E-24, -3.689471323E-24 }
			);
			var expected = new GeographicCoordinate(0.876312568, 0.294084);
			var input = new Point2(6050538.71, 5568990.91);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000004);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.000001);
		}

		[Test]
		public void Espg_1_3_2_4_Test()
		{
			var projection = new KrovakModifiedNorth(
				new GeographicCoordinate(0.863937979, 0.741764932),
				1.370083463,
				0.528627762,
				0.9999,
				new Vector2(5000000, 5000000),
				new SpheroidEquatorialInvF(6377397.155, 299.15281),
				new Point2(1089000, 654000),
				new[] { 2.946529277E-02, 2.515965696E-02, 1.193845912E-07, -4.668270147E-07, 9.233980362E-12, 1.523735715E-12, 1.696780024E-18, 4.408314235E-18, -8.331083518E-24, -3.689471323E-24 }
			);
			var input = new GeographicCoordinate(0.876312568, 0.294084);
			var expected = new Point2(-5568990.91, - 6050538.71);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.01);
			Assert.AreEqual(expected.Y, result.Y, 0.01);
		}

		[Test]
		public void Espg_1_3_2_4_InverseTest()
		{
			var projection = new KrovakModifiedNorth(
				new GeographicCoordinate(0.863937979, 0.741764932),
				1.370083463,
				0.528627762,
				0.9999,
				new Vector2(5000000, 5000000),
				new SpheroidEquatorialInvF(6377397.155, 299.15281),
				new Point2(1089000, 654000),
				new[] { 2.946529277E-02, 2.515965696E-02, 1.193845912E-07, -4.668270147E-07, 9.233980362E-12, 1.523735715E-12, 1.696780024E-18, 4.408314235E-18, -8.331083518E-24, -3.689471323E-24 }
			);
			var expected = new GeographicCoordinate(0.876312568, 0.294084);
			var input = new Point2(-5568990.91, -6050538.71);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.0000000004);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.01);
		}

	}
}
