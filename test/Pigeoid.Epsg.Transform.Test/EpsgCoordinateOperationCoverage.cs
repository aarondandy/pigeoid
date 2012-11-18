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

			Assert.Inconclusive(NoSampleData);
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

			Assert.Inconclusive(NoSampleData);
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

			var expected5705 = 100;
			var expected5797 = expected5705 + 26.3;

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

			var opPath = PathGenerator.Generate(fromCrs, toCrs);
			Assert.IsNotNull(opPath);
			var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
			Assert.IsNotNull(transformation);

			var input = new GeographicCoordinate(42.647992, 3.659603);
			var expectedResult = new GeographicCoordinate(42.649117, -0.026658);

			var result = transformation.TransformValue(input);

			Assert.AreEqual(expectedResult.Latitude, result.Latitude, 0.00005);
			Assert.AreEqual(expectedResult.Longitude, result.Longitude, 0.00005);
		}

	}

}
