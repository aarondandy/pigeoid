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

	}
}
