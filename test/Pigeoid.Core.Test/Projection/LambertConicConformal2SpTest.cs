using NUnit.Framework;
using Pigeoid.Projection;
using Vertesaur;

namespace Pigeoid.Core.Test.Projection
{
	[TestFixture]
	public class LambertConicConformal2SpTest
	{

		[Test]
		public void EpsgExample1311Test()
		{
			var proj = new LambertConicConformal2Sp(
				new GeographicCoordinate(0.48578331, -1.72787596), 
				0.49538262,
				0.52854388,
				new Vector2(2000000.0, 0),
				new SpheroidEquatorialInvF(20925832.16, 294.97870)
			);
			var a = new GeographicCoordinate(0.49741884, -1.67551608);
			var b = new Point2(2963503.91, 254759.80);

			var projected = proj.TransformValue(a);
			Assert.AreEqual(b.X, projected.X, 0.05);
			Assert.AreEqual(b.Y, projected.Y, 0.04);
		}

		[Test]
		public void EpsgExample1311InverseTest()
		{
			var proj = new LambertConicConformal2Sp(
				new GeographicCoordinate(0.48578331, -1.72787596),
				0.49538262,
				0.52854388,
				new Vector2(2000000.0, 0),
				new SpheroidEquatorialInvF(20925832.16, 294.97870)
			);
			var a = new GeographicCoordinate(0.49741884, -1.67551608);
			var b = new Point2(2963503.91, 254759.80);

			var unProjected = proj.GetInverse().TransformValue(b);

			Assert.AreEqual(a.Latitude, unProjected.Latitude, 0.000000002);
			Assert.AreEqual(a.Longitude, unProjected.Longitude, 0.000000003);
		}

	}
}
