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
	public class HyperbolicCassiniSoldnerTest
	{

		[Test]
		public void EpsgExample1341Test()
		{
			var projection = new HyperbolicCassiniSoldner(
				new GeographicCoordinate(-0.283616003, 3.129957125), 
				new Vector2(12513.318, 16628.885),
				new SpheroidEquatorialInvF(317063.667, 293.4663077)
			);

			var projected = projection.TransformValue(new GeographicCoordinate(-0.293938867, 3.141493807));

			Assert.AreEqual(16015.2890, projected.X, 0.08);
			Assert.AreEqual(13369.6601, projected.Y, 0.06);
		}

		[Test]
		public void EpsgExample1341InverseTest()
		{
			var projection = new HyperbolicCassiniSoldner(
				new GeographicCoordinate(-0.283616003, 3.129957125),
				new Vector2(12513.318, 16628.885),
				new SpheroidEquatorialInvF(317063.667, 293.4663077)
			);

			var unProjected = projection.GetInverse().TransformValue(new Point2(16015.2890, 13369.6601));

			Assert.AreEqual(-0.293938867, unProjected.Latitude, 0.000006);
			Assert.AreEqual(3.141493807, unProjected.Longitude, 0.00000002);
		}

	}
}
