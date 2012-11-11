using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.Unit;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Epsg.Transform.Test
{
	[TestFixture]
	public class EpsgCoordinateOperationCoverage
	{

		private static void AreEqual(GeographicCoordinate expected, GeographicCoordinate actual, double delta) {
			Assert.AreEqual(expected.Latitude, actual.Latitude, delta);
			Assert.AreEqual(expected.Longitude, actual.Longitude, delta);
		}

		private static void AreEqual(GeographicHeightCoordinate expected, GeographicHeightCoordinate actual, double delta) {
			Assert.AreEqual(expected.Latitude, actual.Latitude, delta);
			Assert.AreEqual(expected.Longitude, actual.Longitude, delta);
			Assert.AreEqual(expected.Height, actual.Height, delta);
		}

		private static void AreEqual(Point2 expected, Point2 actual, double delta) {
			Assert.AreEqual(expected.X, actual.X, delta);
			Assert.AreEqual(expected.Y, actual.Y, delta);
		}

		private static void AreEqual(Point3 expected, Point3 actual, double delta) {
			Assert.AreEqual(expected.X, actual.X, delta);
			Assert.AreEqual(expected.Y, actual.Y, delta);
			Assert.AreEqual(expected.Z, actual.Z, delta);
		}

		public EpsgCrsCoordinateOperationPathGenerator PathGenerator;
		public BasicCoordinateOperationToTransformationGenerator TransformationGenerator;
		public IUnitConversion<double> DegreesToRadians;
		public GeographicCoordinate DenverWgs84Degrees;
		public GeographicCoordinate MadridWgs84Degrees;

		[TestFixtureSetUp]
		public void TestFixtureSetUp() {
			PathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
			TransformationGenerator = new BasicCoordinateOperationToTransformationGenerator();
			DegreesToRadians = SimpleUnitConversionGenerator.FindConversion(EpsgUnit.Get(9102), EpsgUnit.Get(9101));
			DenverWgs84Degrees = new GeographicCoordinate(39.739167, -104.984722);
			MadridWgs84Degrees = new GeographicCoordinate(40.383333, -3.716667);
		}

		[Test]
		public void m1024_popularVisualisationPseudoMercator() {
			// method: 1024
			// op: 3856
			// crs: 3857 to 4326
			var opPath = PathGenerator.Generate(EpsgCrs.Get(3857), EpsgCrs.Get(4326));
			Assert.IsNotNull(opPath);
			var transformation = TransformationGenerator.Create(opPath) as ITransformation<Point2, GeographicCoordinate>;
			Assert.IsNotNull(transformation);
			Assert.That(transformation.HasInverse);
			var inverse = transformation.GetInverse();
			Assert.IsNotNull(inverse);

			var projected = new Point2(-11686845, 4828110); // make sure this is meters
			var geographic = DenverWgs84Degrees; // this should be converted to radians implicitly

			AreEqual(geographic, transformation.TransformValue(projected), 0.00001);
			AreEqual(projected, inverse.TransformValue(geographic), 1);

		}

		[Test, Ignore]
		public void m1025_gographic3DToGravityRelatedHeightEgm2008() {
			// method: 1025
			// op: 3858 or 3859
			// crs: 4979 to 3855
			var opPath = PathGenerator.Generate(EpsgCrs.Get(4979), EpsgCrs.Get(3855));
			Assert.IsNotNull(opPath);
			var transformation = TransformationGenerator.Create(opPath) as ITransformation<Point2, GeographicCoordinate>;
			Assert.IsNotNull(transformation);
			Assert.That(transformation.HasInverse);
			var inverse = transformation.GetInverse();
			Assert.IsNotNull(inverse);

			Assert.Inconclusive("Uhhh...   ?");
		}

		[Test]
		public void m1026_mercatorSpherical() {
			// method: 1026
			// op:
			// crs:
			Assert.Inconclusive("Can't find a use of this operation.");
		}

		[Test]
		public void m1027_lambertAzimuthalEqualAreaSpherical() {
			// method: 1027
			// op: 3999
			// crs: 4023 to 4026

			var opPath = PathGenerator.Generate(EpsgCrs.Get(4023), EpsgCrs.Get(4026));
			Assert.IsNotNull(opPath);
			var transformation = TransformationGenerator.Create(opPath) as ITransformation<Point2, GeographicCoordinate>;
			Assert.IsNotNull(transformation);
			Assert.That(transformation.HasInverse);
			var inverse = transformation.GetInverse();
			Assert.IsNotNull(inverse);

			Assert.Inconclusive("Need some sample points.");
		}

	}
}
