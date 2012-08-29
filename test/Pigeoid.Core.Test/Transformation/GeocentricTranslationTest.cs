using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pigeoid.Transformation;
using Vertesaur;

namespace Pigeoid.Core.Test.Transformation
{
	[TestFixture]
	public class GeocentricTranslationTest
	{

		[Test]
		public void EpsgExample2431Test()
		{
			var s = new Point3(3771793.97, 140253.34, 5124304.35);
			var t = new Point3(3771878.84, 140349.83, 5124421.30);
			var transform = new GeocentricTranslation(new Vector3(84.87, 96.49, 116.95));

			var result = transform.TransformValue(s);

			Assert.AreEqual(t.X, result.X, 0.000000001);
			Assert.AreEqual(t.Y, result.Y, 0.0);
			Assert.AreEqual(t.Z, result.Z, 0.0);
		}

		[Test]
		public void EpsgExample2431InverseTest()
		{
			var s = new Point3(3771793.97, 140253.34, 5124304.35);
			var t = new Point3(3771878.84, 140349.83, 5124421.30);
			var transform = new GeocentricTranslation(new Vector3(84.87, 96.49, 116.95));

			var result = transform.GetInverse().TransformValue(t);

			Assert.AreEqual(s.X, result.X, 0.000000001);
			Assert.AreEqual(s.Y, result.Y, 0.0);
			Assert.AreEqual(s.Z, result.Z, 0.0);
		}

	}
}
