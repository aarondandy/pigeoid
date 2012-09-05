using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{

	[TestFixture]
	public class MercatorTest
	{

		[Test]
		public void MapProjectionsWorkingManualClarkeLatitudeTableTest()
		{
			var clarke1866 = new SpheroidEquatorialPolar(6378206.4, 6356583.8);
			var projection = new Mercator(0, 1, Vector2.Zero, new SpheroidEquatorialInvF(1.0, clarke1866.InvF)); // it has a major axis of 1.0

			// check all the Y coordinates
			Assert.AreEqual(3.12454, projection.TransformValue(new GeographicCoordinate(1.48352986, 0)).Y, 0.001); // 85 degrees N
			Assert.AreEqual(1.50031, projection.TransformValue(new GeographicCoordinate(1.13446401, 0)).Y, 0.001); // 65 degrees N
			Assert.AreEqual(0.87658, projection.TransformValue(new GeographicCoordinate(0.785398163, 0)).Y, 0.001); // 45 degrees N
			Assert.AreEqual(0.44801, projection.TransformValue(new GeographicCoordinate(0.436332313, 0)).Y, 0.001); // 25 degrees N
			Assert.AreEqual(0.26309, projection.TransformValue(new GeographicCoordinate(0.261799388, 0)).Y, 0.001); // 15 degrees N
			Assert.AreEqual(0.08679, projection.TransformValue(new GeographicCoordinate(0.0872664626, 0)).Y, 0.001); // 5 degrees N
			Assert.AreEqual(0, projection.TransformValue(new GeographicCoordinate(0, 0)).Y, 0.001); // 0 degrees
		}

		[Test]
		public void MapProjectionsWorkingManualClarkeLatitudeInverseTableTest()
		{
			var clarke1866 = new SpheroidEquatorialPolar(6378206.4, 6356583.8);
			var projection = new Mercator(0, 1, Vector2.Zero, new SpheroidEquatorialInvF(1.0, clarke1866.InvF))  // it has a major axis of 1.0
				.GetInverse();

			// check all the Y coordinates
			Assert.AreEqual(1.48352986, projection.TransformValue(new Point2(0, 3.12454)).Latitude, 0.001); // 85 degrees N
			Assert.AreEqual(1.13446401, projection.TransformValue(new Point2(0, 1.50031)).Latitude, 0.001); // 65 degrees N
			Assert.AreEqual(0.785398163, projection.TransformValue(new Point2(0, 0.87658)).Latitude, 0.001); // 45 degrees N
			Assert.AreEqual(0.436332313, projection.TransformValue(new Point2(0, 0.44801)).Latitude, 0.001); // 25 degrees N
			Assert.AreEqual(0.261799388, projection.TransformValue(new Point2(0, 0.26309)).Latitude, 0.001); // 15 degrees N
			Assert.AreEqual(0.0872664626, projection.TransformValue(new Point2(0, 0.08679)).Latitude, 0.001); // 5 degrees N
			Assert.AreEqual(0, projection.TransformValue(new Point2(0, 0)).Latitude, 0.001); // 0 degrees
		}

		[Test]
		public void EpsgExample_1_3_3_1_Test()
		{
			var projection = new Mercator(
				1.91986218,
				0.997,
				new Vector2(3900000, 900000),
				new SpheroidEquatorialInvF(6377397.155, 299.15281)
			);
			var input = new GeographicCoordinate(-0.05235988, 2.09439510);
			var expected = new Point2(5009726.58, 569150.82);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.03);
			Assert.AreEqual(expected.Y, result.Y, 0.02);
		}

		[Test]
		public void EpsgExample_1_3_3_1_InverseTest()
		{
			var projection = new Mercator(
				1.91986218,
				0.997,
				new Vector2(3900000, 900000),
				new SpheroidEquatorialInvF(6377397.155, 299.15281)
			);
			var expected = new GeographicCoordinate(-0.05235988, 2.09439510);
			var input = new Point2(5009726.58, 569150.82);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.000000003);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.000000005);
		}

		[Test]
		public void EpsgExample_1_3_3_2_Test()
		{
			var projection = new Mercator(
				new GeographicCoordinate(0.73303829, 0.89011792), 
				new Vector2(0, 0),
				new SpheroidEquatorialInvF(6378245, 298.3)
			);
			var input = new GeographicCoordinate(0.9250245, 0.9250245);
			var expected = new Point2(165704.29, 5171848.07);

			var result = projection.TransformValue(input);

			Assert.AreEqual(expected.X, result.X, 0.03);
			Assert.AreEqual(expected.Y, result.Y, 0.05);
		}

		[Test]
		public void EpsgExample_1_3_3_2_InverseTest()
		{
			var projection = new Mercator(
				new GeographicCoordinate(0.73303829, 0.89011792),
				new Vector2(0, 0),
				new SpheroidEquatorialInvF(6378245, 298.3)
			);
			var expected = new GeographicCoordinate(0.9250245, 0.9250245);
			var input = new Point2(165704.29, 5171848.07);

			var result = projection.GetInverse().TransformValue(input);

			Assert.AreEqual(expected.Latitude, result.Latitude, 0.000000006);
			Assert.AreEqual(expected.Longitude, result.Longitude, 0.000000005);
		}

	}
}
