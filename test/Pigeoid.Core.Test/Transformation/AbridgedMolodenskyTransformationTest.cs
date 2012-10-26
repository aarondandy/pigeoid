using NUnit.Framework;
using Pigeoid.Transformation;
using Vertesaur;

namespace Pigeoid.Core.Test.Transformation
{
	[TestFixture]
	public class AbridgedMolodenskyTransformationTest
	{

		[Test]
		public void EpsgExample2442Test()
		{
			var transform = new AbridgedMolodenskyTransformation(
				new Vector3(84.87, 96.49, 116.95),
				new SpheroidEquatorialInvF(6378137.0, 298.2572236),
				new SpheroidEquatorialInvF(6378388.0, 297.0)
			);
			var s = new GeographicHeightCoordinate(0.939151102, 0.037167659, 73);
			var t = new GeographicHeightCoordinate(0.93916441, 0.03719237, 28.02);

			var result = transform.TransformValue(s);

			Assert.AreEqual(t.Latitude, result.Latitude, 0.000005);
			Assert.AreEqual(t.Longitude, result.Longitude, 0.000005);
			Assert.AreEqual(t.Height, result.Height, 0.08);

		}

		[Test]
		public void EpsgExample2442InverseTest()
		{
			var transform = new AbridgedMolodenskyTransformation(
				new Vector3(84.87, 96.49, 116.95),
				new SpheroidEquatorialInvF(6378137.0, 298.2572236),
				new SpheroidEquatorialInvF(6378388.0, 297.0)
			);
			var s = new GeographicHeightCoordinate(0.939151102, 0.037167659, 73);
			var t = new GeographicHeightCoordinate(0.93916441, 0.03719237, 28.02);

			var result = transform.GetInverse().TransformValue(t);

			Assert.AreEqual(s.Latitude, result.Latitude, 0.000003);
			Assert.AreEqual(s.Longitude, result.Longitude, 0.000005);
			Assert.AreEqual(s.Height, result.Height, 0.08);

		}

	}
}
