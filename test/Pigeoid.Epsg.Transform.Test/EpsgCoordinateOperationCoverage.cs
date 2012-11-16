using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.CoordinateOperationCompilation;
using Pigeoid.Unit;
using Vertesaur;
using Vertesaur.Contracts;
using Vertesaur.Transformation;

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

		private static ITransformation<TFrom,TTo> CreateTyped<TFrom,TTo>(ITransformation transformation){
			if(transformation == null)
				return null;
			if (transformation is ITransformation<TFrom, TTo>)
				return transformation as ITransformation<TFrom, TTo>;
			if (transformation is ConcatenatedTransformation)
				return new ConcatenatedTransformation<TFrom, TTo>(((ConcatenatedTransformation)(transformation)).Transformations);
			return null;
		}

		public EpsgCrsCoordinateOperationPathGenerator PathGenerator;
		public StaticCoordinateOperationCompiler StaticCompiler;
		public IUnitConversion<double> DegreesToRadians;
		public GeographicCoordinate DenverWgs84Degrees;
		public GeographicCoordinate MadridWgs84Degrees;

		[TestFixtureSetUp]
		public void TestFixtureSetUp() {
			PathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
			StaticCompiler = new StaticCoordinateOperationCompiler();
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
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			
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
			var transformation = StaticCompiler.Compile(opPath) as ITransformation<Point2, GeographicCoordinate>;
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
			// op: 3899
			// crs: 4052 to 2163

			var opPath = PathGenerator.Generate(EpsgCrs.Get(4052), EpsgCrs.Get(2163));
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);
			Assert.That(transformation.HasInverse);
			var inverse = transformation.GetInverse();
			Assert.IsNotNull(inverse);

			var geographic = DenverWgs84Degrees;
			var projected = new Point2(-426348.460028, -571951.34121900005);

			AreEqual(projected, transformation.TransformValue(geographic), 1);
			AreEqual(geographic, inverse.TransformValue(projected), 0.00001);
		}

		[Test]
		public void m1028_equidistantCylindrical() {
			// method: 1028
			// op: 4085
			// crs: 4087 to 4326

			var opPath = PathGenerator.Generate(EpsgCrs.Get(4087), EpsgCrs.Get(4326));
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);
			Assert.That(transformation.HasInverse);
			var inverse = transformation.GetInverse();
			Assert.IsNotNull(inverse);

			var expectedGeographic = DenverWgs84Degrees;
			var expectedProjected = new Point2(0, 0);

			var actualGeographic = transformation.TransformValue(expectedProjected);
			var actualprojected = inverse.TransformValue(expectedGeographic);

			Assert.Inconclusive("Need a sample point.");
		}

	}
}
