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

		private const string NoUsages = "Can't find a use of this operation method.";
		private const string NoSampleData = "No sample data to test with.";
		private const string NotSupported = "This operation method is not yet supported.";

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

		[Test, Ignore(NotSupported)]
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

			Assert.Inconclusive(NoSampleData);
		}

		[Test, Ignore(NoUsages)]
		public void m1026_mercatorSpherical() {
			Assert.Inconclusive(NoSampleData);
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

			var expected4326 = new GeographicCoordinate(55, 10);
			var expected4087 = new Point2(1113194.91, 6097230.31);

			AreEqual(expected4326, transformation.TransformValue(expected4087), 0.0000003);
			AreEqual(expected4087, inverse.TransformValue(expected4326), 0.004);
		}

		[Test, Ignore(NoSampleData)]
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

		}

		[Test, Ignore(NotSupported)]
		public void m1030_geographic3DToGravityRelatedHeight_nz_geoid2009(){
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NoUsages)]
		public void m1031_geocentricTranslations_geocentricDomain() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m1032_coordinateFrameRotation_geocentricDomain() {
			Assert.Inconclusive(NoUsages);
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
		}

		[Test, Ignore(NoUsages)]
		public void m1034_molodenskyBadekas_geocentricDomain() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m1035_geocentricTranslations_geog3DDomain() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m1036_cartesianGridOffsetsFromFormFunction() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m1037_positionVectorTransformation_geog3DDomain() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m1038_coordinateFrameRotation_geog3DDomain() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m1039_molodenskyBadekas_geog3DDomain() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NotSupported)]
		public void m1040_gntrans() {
			Assert.Inconclusive(NotSupported);
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

			var expected4818 = new GeographicCoordinate(50.209012, 16.849772);
			var expected5221 = new Point2(-568991.00, -1050538.63);

			AreEqual(expected4818, transformation.TransformValue(expected5221), 0.000001);
			AreEqual(expected5221, inverse.TransformValue(expected4818), 0.1);
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

			var expected5229 = new GeographicCoordinate(50.209012, 16.849772);
			var exoected5224 = new Point2(6050538.71, 5568990.91);

			AreEqual(expected5229, transformation.TransformValue(exoected5224), 0.000001);
			AreEqual(exoected5224, inverse.TransformValue(expected5229), 0.1);
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

			var expected5229 = new GeographicCoordinate(50.209012, 16.849772);
			var expected5225 = new Point2(-5568990.91, -6050538.71);

			AreEqual(expected5229, transformation.TransformValue(expected5225), 0.000001);
			AreEqual(expected5225, inverse.TransformValue(expected5229), 0.1);
		}

		[Test, Ignore(NoUsages)]
		public void m1044_mercatorVariantC() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NotSupported)]
		public void m1045_geographic3DToGravityRelatedHeightOsgm02Ire() {
			Assert.Inconclusive(NotSupported);
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

		[Test, Ignore(NoUsages)]
		public void m9602_geographicGeocentricConversions() {
			Assert.Inconclusive(NoUsages);
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

		[Test, Ignore(NoUsages)]
		public void m9604_molodensky(){
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m9605_abridgedMolodensky() {
			Assert.Inconclusive(NoUsages);
		}

		[Test]
		public void m9606_positionVectorTransformation_geog2DDomain() {
			// method: 9606
			// op: 1643
			// crs: 4181 to 4326

			var fromCrs = EpsgCrs.Get(4181);
			var toCrs = EpsgCrs.Get(4326);

			var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator(new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(
				x => !x.Deprecated,
				x => {
					var coi = x as EpsgCoordinateOperationInfo;
					if (coi != null){
						if (coi.Method.Code != 9606)
							return false;
					}
					return true;
				}));

			var opPath = pathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = pathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(invOpPath);

			var expected4181 = new GeographicCoordinate(49.843933, 6.128542);
			var expected4326 = new GeographicCoordinate(49.845, 6.13);

			AreEqual(expected4326, transformation.TransformValue(expected4181), 0.002);
			AreEqual(expected4181, inverse.TransformValue(expected4326), 0.002);
		}

		[Test]
		public void m9607_coordinateFrameRotation_geog2DDomain(){
			// method: 9607
			// op: 5486
			// crs: 4181 to 4326

			var fromCrs = EpsgCrs.Get(4181);
			var toCrs = EpsgCrs.Get(4326);

			var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator(new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(
				x => !x.Deprecated,
				x => {
					var coi = x as EpsgCoordinateOperationInfo;
					if (coi != null) {
						if (coi.Method.Code != 9607)
							return false;
					}
					return true;
				}));

			var opPath = pathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = pathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(invOpPath);

			var expected4181 = new GeographicCoordinate(49.843933, 6.128542);
			var expected4326 = new GeographicCoordinate(49.845, 6.13);

			AreEqual(expected4326, transformation.TransformValue(expected4181), 0.002);
			AreEqual(expected4181, inverse.TransformValue(expected4326), 0.002);
		}

		[Test, Ignore(NotSupported)]
		public void m9613_nadcon() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9614_ntV1() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9615_ntv2() {
			Assert.Inconclusive(NotSupported);
		}

		[Test]
		public void m9616_verticalOffset() {
			// method: 9616
			// op: 5443
			// crs: 5705 to 5797

			var fromCrs = EpsgCrs.Get(5705);
			var toCrs = EpsgCrs.Get(5797);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<double, double>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<double, double>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			const int expected5705 = 100;
			const double expected5797 = expected5705 + 26.3;

			Assert.AreEqual(expected5797, transformation.TransformValue(expected5705));
			Assert.AreEqual(expected5705, inverse.TransformValue(expected5797));
		}

		[Test]
		public void m9617_madridToEd50Polynomial() {
			// method: 9617
			// op: 1026
			// crs: 4903 to 4230

			var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator(new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(
				x => !x.Deprecated,
				x => {
					var coi = x as EpsgCoordinateOperationInfo;
					return coi == null || coi.Method.Code == 9617;
				}));

			var fromCrs = EpsgCrs.Get(4903);
			var toCrs = EpsgCrs.Get(4230);

			var opPath = pathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var input = new GeographicCoordinate(42.647992, 3.659603);
			var expectedResult = new GeographicCoordinate(42.649117, -0.026658);

			var result = transformation.TransformValue(input);

			Assert.AreEqual(expectedResult.Latitude, result.Latitude, 0.00005);
			Assert.AreEqual(expectedResult.Longitude, result.Longitude, 0.00005);
		}

		[Test, Ignore(NotSupported)]
		public void m9618_geographic2DWithHeightOffset(){
			// method: 9618
			// op: 1335
			// crs: 4301 to 4326

			var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator(new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(
				x => !x.Deprecated,
				x => {
					var coi = x as EpsgCoordinateOperationInfo;
					return coi == null || coi.Method.Code == 9618;
				}));

			var fromCrs = EpsgCrs.Get(4301);
			var toCrs = EpsgCrs.Get(4326);

			var opPath = pathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicHeightCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = pathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicHeightCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			Assert.Inconclusive(NotSupported);
		}

		[Test]
		public void m9619_geographic2DOffsets(){
			// method: 9619
			// op: 1891
			// crs: 4120 to 4121

			var fromCrs = EpsgCrs.Get(4120);
			var toCrs = EpsgCrs.Get(4121);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected4120 = new GeographicCoordinate(38.14349, 23.80451);
			var expected4121 = new GeographicCoordinate(38.141863, 23.804588);

			AreEqual(expected4120, transformation.TransformValue(expected4121), 0.004);
			AreEqual(expected4121, inverse.TransformValue(expected4120), 0.004);
		}

		[Test, Ignore(NotSupported)]
		public void m9620_norwayOffshoreInterpolation(){
			Assert.Ignore(NotSupported);
		}

		[Test]
		public void m9621_similarityTransformation(){
			// method: 9621
			// op: 5166
			// crs: 23031 to 25831

			var fromCrs = EpsgCrs.Get(23031);
			var toCrs = EpsgCrs.Get(25831);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, Point2>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<Point2, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected23031 = new Point2(300000, 4500000);
			var expected25831 = new Point2(299905.060, 4499796.515);

			AreEqual(expected25831, transformation.TransformValue(expected23031), 0.001);
			AreEqual(expected23031, inverse.TransformValue(expected25831), 0.001);
		}

		[Test, Ignore(NoUsages)]
		public void m9622_affineOrthogonalGeometricTransformation(){
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m9623_affineGeometricTransformation(){
			Assert.Inconclusive(NoUsages);
		}

		[Test]
		public void m9624_affineParametricTransformation(){
			// method: 9624
			// op: 15857
			// crs: 3367 to 3343

			var fromCrs = EpsgCrs.Get(3367);
			var toCrs = EpsgCrs.Get(3343);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, Point2>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<Point2, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected3367 = new Point2(500000, 2324163.285331);
			var expected3343 = new Point2(500000, 2324361.095694);

			AreEqual(expected3343, transformation.TransformValue(expected3367), 300);
			AreEqual(expected3367, inverse.TransformValue(expected3343), 300);
		}

		[Test, Ignore(NoUsages)]
		public void m9625_generalPolynomial2NdOrder() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m9626_generalPolynomial3RdOrder() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m9627_generalPolynomial4ThOrder() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m9628_reversiblePolynomial2NdOrder() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NoUsages)]
		public void m9629_reversiblePolynomial3RdOrder() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NotSupported)]
		public void m9630_reversiblePolynomial4ThOrder() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9631_complexPolynomial3RdOrder() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9632_complexPolynomial4ThOrder() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9633_ordnanceSurveyNationalTransformation() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9634_maritimeProvincesPolynomialInterpolation() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9635_geographic3DToGeographic2D_gravityRelatedHeight() {
			Assert.Inconclusive(NotSupported);
		}

		[Test]
		public void m9636_molodenskyBadekas_geog2DDomain() {
			// method: 9636
			// op: 5484
			// crs: 4181 to 4326

			var fromCrs = EpsgCrs.Get(4181);
			var toCrs = EpsgCrs.Get(4326);

			var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator(new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(
				x => !x.Deprecated,
				x => {
					var coi = x as EpsgCoordinateOperationInfo;
					if (coi != null) {
						if (coi.Method.Code != 9636)
							return false;
					}
					return true;
				}));

			var opPath = pathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = pathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected4181 = new GeographicCoordinate(49.843933, 6.128542);
			var expected4326 = new GeographicCoordinate(49.845, 6.13);

			AreEqual(expected4326, transformation.TransformValue(expected4181), 0.0001);
			AreEqual(expected4181, inverse.TransformValue(expected4326), 0.000001);
		}

		[Test, Ignore(NotSupported)]
		public void m9637_degreeRepresentationConversion_deg_dmsh() {
			Assert.Inconclusive(NotSupported);
		}
		[Test, Ignore(NotSupported)]
		public void m9638_degreeRepresentationConversion_degH_dmsh() {
			Assert.Inconclusive(NotSupported);
		}
		[Test, Ignore(NotSupported)]
		public void m9639_degreeRepresentationConversion_hDeg_dmsh() {
			Assert.Inconclusive(NotSupported);
		}
		[Test, Ignore(NotSupported)]
		public void m9640_degreeRepresentationConversion_dm_dmsh() {
			Assert.Inconclusive(NotSupported);
		}
		[Test, Ignore(NotSupported)]
		public void m9641_degreeRepresentationConversion_dmh_dmsh() {
			Assert.Inconclusive(NotSupported);
		}
		[Test, Ignore(NotSupported)]
		public void m9642_degreeRepresentationConversion_hDm_dmsh() {
			Assert.Inconclusive(NotSupported);
		}
		[Test, Ignore(NotSupported)]
		public void m9643_degreeRepresentationConversion_dms_dmsh() {
			Assert.Inconclusive(NotSupported);
		}
		[Test, Ignore(NotSupported)]
		public void m9644_degreeRepresentationConversion_hDms_dmsh() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9645_generalPolynomialOfDegree2() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9646_generalPolynomialOfDegree3() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9647_generalPolynomialOfDegree4() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9648_generalPolynomialOfDegree6() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9649_reversiblePolynomialOfDegree2() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9650_reversiblePolynomialOfDegree3() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9651_reversiblePolynomialOfDegree4() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9652_complexPolynomialOfDegree3() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9653_complexPolynomialOfDegree4() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9654_reversiblePolynomialOfDegree13() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9655_franceGeocentricInterpolation() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9656_cartesianGridOffsets() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9657_verticalOffsetAndSlope() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9658_vertcon() {
			Assert.Inconclusive(NotSupported);
		}

		[Test]
		public void m9659_geographic3DTo2D() {
			// method: 9659
			// op: 15539
			// crs: 4645 to 4969

			var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator(new EpsgCrsCoordinateOperationPathGenerator.SharedOptionsAreaPredicate(
				x => !x.Deprecated,
				x =>  !(x is EpsgCoordinateOperationInfo) || ((EpsgCoordinateOperationInfo)x).Method.Code == 9659
			));

			var fromCrs = EpsgCrs.Get(4645);
			var toCrs = EpsgCrs.Get(4969);

			var opPath = pathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicHeightCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = pathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, GeographicHeightCoordinate>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected4645 = new GeographicHeightCoordinate(-20.36, 165.49, 42);
			var expected4969 = new GeographicCoordinate(expected4645.Latitude, expected4645.Longitude);

			AreEqual(expected4969, transformation.TransformValue(expected4645), 0);
			AreEqual(new GeographicHeightCoordinate(expected4645.Latitude, expected4645.Longitude, 0), inverse.TransformValue(expected4969), 0);
		}

		[Test, Ignore(NoUsages)]
		public void m9660_geographic3DOffsets() {
			Assert.Inconclusive(NoUsages);
		}

		[Test, Ignore(NotSupported)]
		public void m9661_geographic3DToGravityRelatedHeightEgm() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9662_geographic3DToGravityRelatedHeightAusgeoid98() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9663_geographic3DToGravityRelatedHeightOsgm02Gb() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9664_geographic3DToGravityRelatedHeightIgn() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9665_geographic3DToGravityRelatedHeightUs() {
			Assert.Inconclusive(NotSupported);
		}

		[Test, Ignore(NotSupported)]
		public void m9666_p6IEqualsJPlus90DegreesSeismicBinGridTransformation() {
			Assert.Inconclusive(NotSupported);
		}

		[Test]
		public void m9801_lambertConicConformal1Sp() {
			// method: 9801
			// op: 19910
			// crs: 24200 to 4242

			var fromCrs = EpsgCrs.Get(24200);
			var toCrs = EpsgCrs.Get(4242);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected24200 = new Point2(255966.58, 142493.51);
			var expected4242 = new GeographicCoordinate(17.932167,-76.943683);

			AreEqual(expected4242, transformation.TransformValue(expected24200), 0.000001);
			AreEqual(expected24200, inverse.TransformValue(expected4242), 0.04);
		}

		[Test]
		public void m9802_lambertConicConformal2Sp() {
			// method: 9802
			// op: 14204
			// crs: 32040 to 4267

			var fromCrs = EpsgCrs.Get(32040);
			var toCrs = EpsgCrs.Get(4267);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected32040 = new Point2(2963503.91, 254759.80);
			var expected4267 = new GeographicCoordinate(28.5,-96);

			AreEqual(expected4267, transformation.TransformValue(expected32040), 0.000000009);
			AreEqual(expected32040, inverse.TransformValue(expected4267), 0.003);
		}

		[Test]
		public void m9803_lamberConicConformal2SpBelgium() {
			// method: 9803
			// op: 19902
			// crs: 31300 to 4313

			var fromCrs = EpsgCrs.Get(31300);
			var toCrs = EpsgCrs.Get(4313);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected31300 = new Point2(251763.20, 153034.13);
			var expected4313 = new GeographicCoordinate(50.679573, 5.80737);

			AreEqual(expected4313, transformation.TransformValue(expected31300), 0.0000006);
			AreEqual(expected31300, inverse.TransformValue(expected4313), 0.06);
		}

		[Test]
		public void m9804_mercator_variantA() {
			// method: 9804
			// op: 19905
			// crs: 3002 to 4257

			var fromCrs = EpsgCrs.Get(3002);
			var toCrs = EpsgCrs.Get(4257);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected5331 = new Point2(5009726.58, 569150.82);
			var expected4804 = new GeographicCoordinate(-3, 120);

			AreEqual(expected4804, transformation.TransformValue(expected5331), 0.00000003);
			AreEqual(expected5331, inverse.TransformValue(expected4804), 0.004);
		}

		[Test]
		public void m9805_mercator_variantB() {
			// method: 9805
			// op: 19884
			// crs: 3388 to 4284

			var fromCrs = EpsgCrs.Get(3388);
			var toCrs = EpsgCrs.Get(4284);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected3388 = new Point2(165704.29, 5171848.07);
			var expected4284 = new GeographicCoordinate(53,53);

			AreEqual(expected4284, transformation.TransformValue(expected3388), 0.00000004);
			AreEqual(expected3388, inverse.TransformValue(expected4284), 0.004);
		}

		[Test]
		public void m9806_cassiniSoldner() {
			// method: 9806
			// op: 19925
			// crs: 30200 to 4302

			var fromCrs = EpsgCrs.Get(30200);
			var toCrs = EpsgCrs.Get(4302);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected30200 = new Point2(66644.94, 82536.22);
			var expected4302 = new GeographicCoordinate(10, -62);

			AreEqual(expected4302, transformation.TransformValue(expected30200), 0.000003);
			AreEqual(expected30200, inverse.TransformValue(expected4302), 0.002);
		}

		[Test]
		public void m9807_transverseMercator() {
			// method: 9807
			// op: 19916
			// crs: 27700 to 4277

			var fromCrs = EpsgCrs.Get(27700);
			var toCrs = EpsgCrs.Get(4277);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected27700 = new Point2(577274.99, 69740.50);
			var expected4277 = new GeographicCoordinate(50.5,0.5);

			AreEqual(expected4277, transformation.TransformValue(expected27700), 0.0000001);
			AreEqual(expected27700, inverse.TransformValue(expected4277), 0.008);
		}

		[Test]
		public void m9808_transverseMercator_sourceOrientated() {
			// method: 9808
			// op: 17529
			// crs: 2053 to 4148

			var fromCrs = EpsgCrs.Get(2053);
			var toCrs = EpsgCrs.Get(4148);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected2053 = new Point2(-2847342.74, 71984.49);
			var expected4148 = new GeographicCoordinate(25.732028, 28.282633);

			AreEqual(expected4148, transformation.TransformValue(expected2053), 0.0000004);
			AreEqual(expected2053, inverse.TransformValue(expected4148), 0.05);
		}

		[Test]
		public void m9809_obliqueStereographic() {
			// method: 9809
			// op: 19914
			// crs: 28992 to 4289

			var fromCrs = EpsgCrs.Get(28992);
			var toCrs = EpsgCrs.Get(4289);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected28992 = new Point2(196105.283, 557057.739);
			var expected4289 = new GeographicCoordinate(53, 6);

			AreEqual(expected4289, transformation.TransformValue(expected28992), 0.000000004);
			AreEqual(expected28992, inverse.TransformValue(expected4289), 0.0004);
		}

		[Test]
		public void m9810_polarStereographic_variantA() {
			// method: 9810
			// op: 16061
			// crs: 5041 to 4326

			var fromCrs = EpsgCrs.Get(5041);
			var toCrs = EpsgCrs.Get(4326);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected5041 = new Point2(3320416.75, 632668.43);
			var expected4326 = new GeographicCoordinate(73, 44);

			AreEqual(expected4326, transformation.TransformValue(expected5041), 0.00000004);
			AreEqual(expected5041, inverse.TransformValue(expected4326), 0.003);
		}

		[Test, Ignore(NotSupported)]
		public void m9811_newZealandMapGrid() {
			Assert.Ignore(NotSupported);
		}

		[Test, Ignore(NoSampleData)]
		public void m9812_hotineObliqueMercator_variantA() {
			Assert.Inconclusive(NoSampleData);
		}

		[Test]
		public void m9813_labordeObliqueMercator() {
			// method: 9813
			// op: 19861
			// crs: 29701 to 4810

			var fromCrs = EpsgCrs.Get(29701);
			var toCrs = EpsgCrs.Get(4810);

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var invOpPath = PathGenerator.Generate(toCrs, fromCrs);
			Assert.IsNotNull(invOpPath);
			var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
			Assert.IsNotNull(inverse);

			var expected29701 = new Point2(188333.848, 1098841.091);
			var expected4810 = new GeographicCoordinate(-17.9886666667, 46.800381173); // grads

			AreEqual(expected4810, transformation.TransformValue(expected29701), 0.01);
			AreEqual(expected29701, inverse.TransformValue(expected4810), 50);
		}

	}

}
