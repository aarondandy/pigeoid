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
			var crsFrom = EpsgCrs.Get(3857);
			var crsTo = EpsgCrs.Get(4326);

			var opPath = PathGenerator.Generate(crsFrom, crsTo);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(crsTo, crsFrom);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
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
			var fromCrs = EpsgCrs.Get(4979);
			var toCrs = EpsgCrs.Get(3855);
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
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
			var fromCrs = EpsgCrs.Get(4052);
			var toCrs = EpsgCrs.Get(2163);
			
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);
			
			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
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
			var fromCrs = EpsgCrs.Get(4087);
			var toCrs = EpsgCrs.Get(4326);
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			Assert.Inconclusive("Need a sample point.");
		}

		[Test]
		public void m1029_equidistantCylindricalSpherical(){
			// method: 1029
			// op: 4086
			// crs: 4088 to 4047
			var fromCrs = EpsgCrs.Get(4088);
			var toCrs = EpsgCrs.Get(4047);
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);
			
			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			Assert.Inconclusive("Need a sample point.");
		}

		[Test, Ignore]
		public void m1030_geographic3DToGravityRelatedHeight_nz_geoid2009(){
			Assert.Inconclusive("???");
		}

		[Test, Ignore]
		public void m1031_geocentricTranslations_geocentricDomain() {
			Assert.Inconclusive("???");
		}

		[Test, Ignore]
		public void m1032_coordinateFrameRotation_geocentricDomain() {
			Assert.Inconclusive("???");
		}

		[Test]
		public void m1033_positionVectorTransformation_geocentricDomain() {
			// method: 1033
			// op: 5333
			// crs: 4896 to 5332

			var fromCrs = EpsgCrs.Get(4896);
			var toCrs = EpsgCrs.Get(5332);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point3, Point3>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<Point3, Point3>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expectedValue4896 = new Point3(100, 100, ((ICrsGeodetic)(fromCrs)).Datum.Spheroid.A);
			var expectedValue5332 = expectedValue4896;
			var actualValue4896 = transformation.TransformValue(expectedValue5332);
			var actualValue5332 = inverse.TransformValue(expectedValue4896);

			AreEqual(expectedValue4896, actualValue4896, 0.006);
			AreEqual(expectedValue5332, actualValue5332, 0.006);

			Assert.Inconclusive("Need some real test points.");
		}

		[Test, Ignore]
		public void m1034_molodenskyBadekas_geocentricDomain() {
			Assert.Inconclusive("No operations");
		}

		[Test, Ignore]
		public void m1035_geocentricTranslations_geog3DDomain() {
			Assert.Inconclusive("No operations");
		}

		[Test, Ignore]
		public void m1036_cartesianGridOffsetsFromFormFunction() {
			Assert.Inconclusive("not yet supported");
		}

		[Test, Ignore]
		public void m1037_positionVectorTransformation_geog3DDomain() {
			Assert.Inconclusive("No operations");
		}

		[Test, Ignore]
		public void m1038_coordinateFrameRotation_geog3DDomain() {
			Assert.Inconclusive("No operations");
		}

		[Test, Ignore]
		public void m1039_molodenskyBadekas_geog3DDomain() {
			Assert.Inconclusive("No operations");
		}

		[Test, Ignore]
		public void m1040_gntrans() {
			Assert.Inconclusive("not yet supported");
		}

		[Test]
		public void m1041_krovakNorthOrientated() {
			// method: 1041
			// op: 5218
			// crs: 5221 to 4818

			var fromCrs = EpsgCrs.Get(5221);
			var toCrs = EpsgCrs.Get(4818);
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			Assert.Inconclusive("Need a sample point.");
		}

		[Test]
		public void m1042_krovakModified() {
			// method: 1042
			// op: 5219
			// crs: 5224 to 5229

			var fromCrs = EpsgCrs.Get(5224);
			var toCrs = EpsgCrs.Get(5229);
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			Assert.Inconclusive("Need a sample point.");
		}

		[Test]
		public void m1043_krovakModifiedNorthOrientated() {
			// method: 1043
			// op: 5220
			// crs: 5225 to 5229

			var fromCrs = EpsgCrs.Get(5225);
			var toCrs = EpsgCrs.Get(5229);
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			Assert.Inconclusive("Need a sample point.");
		}

		[Test, Ignore]
		public void m1044_mercatorVariantC() {
			Assert.Inconclusive("No operations");
		}

		[Test, Ignore]
		public void m1045_geographic3DToGravityRelatedHeightOsgm02Ire() {
			Assert.Inconclusive("Not supported");
		}

		[Test]
		public void m9601_longitudeRotation() {
			// method: 9601
			// op: 1258
			// crs: 4802 to 4218

			var fromCrs = EpsgCrs.Get(4802);
			var toCrs = EpsgCrs.Get(4218);
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected4802 = new GeographicCoordinate(4.215, 1.095917);
			var expected4218 = new GeographicCoordinate(4.217855, -72.988445);

			AreEqual(expected4218, transformation.TransformValue(expected4802), 0.01);
			AreEqual(expected4802, inverse.TransformValue(expected4218), 0.01);
		}

		[Test, Ignore]
		public void m9602_geographicGeocentricConversions() {
			Assert.Inconclusive("No operations");
		}

		[Test]
		public void m9603_geocentricTranslationsGeog2DDomain() {
			// method: 9603
			// op: 1173
			// crs: 4267 to 4326

			var fromCrs = EpsgCrs.Get(4204);
			var toCrs = EpsgCrs.Get(4326);
			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected4267 = new GeographicCoordinate(39.7391755097579, - 104.984149234204);
			var expected4326 = DenverWgs84Degrees;

			AreEqual(expected4326, transformation.TransformValue(expected4267), 0.005);
			AreEqual(expected4267, inverse.TransformValue(expected4326), 0.005);

		}

	}
}
