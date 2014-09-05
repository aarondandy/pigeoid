using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pigeoid.CoordinateOperation;
using Pigeoid.CoordinateOperation.Projection;
using Pigeoid.Unit;
using Vertesaur;
using Vertesaur.Transformation;
using System;

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

        private static void AreEqual(GeographicHeightCoordinate expected, GeographicHeightCoordinate actual, double angleDelta, double linearDelta) {
            Assert.AreEqual(expected.Latitude, actual.Latitude, angleDelta);
            Assert.AreEqual(expected.Longitude, actual.Longitude, angleDelta);
            Assert.AreEqual(expected.Height, actual.Height, linearDelta);
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

        private static bool FilterForMethodCode(int methodCode, ICoordinateOperationCrsPathInfo opPathInfo) {
            if (opPathInfo != null) {
                foreach (var rawOp in opPathInfo.CoordinateOperations) {

                    var epsgInverseOp = rawOp as EpsgCoordinateOperationInverse;
                    var epsgOpBase = (epsgInverseOp != null)
                        ? epsgInverseOp.Core
                        : rawOp as EpsgCoordinateOperationInfoBase;

                    var catOp = epsgOpBase as EpsgConcatenatedCoordinateOperationInfo;
                    if (catOp != null)
                        return catOp.Steps.Any(x => x.Method.Code == methodCode);

                    var normalOp = epsgOpBase as EpsgCoordinateOperationInfo;
                    if (normalOp != null)
                        return normalOp.Method.Code == methodCode;

                }
            }
            return false;
        }

        private static ITransformation<TFrom, TTo> CreateTyped<TFrom, TTo>(ITransformation transformation) {
            if (transformation == null)
                return null;
            if (transformation is ITransformation<TFrom, TTo>)
                return transformation as ITransformation<TFrom, TTo>;
            if (transformation is ConcatenatedTransformation)
                return new ConcatenatedTransformation<TFrom, TTo>((ConcatenatedTransformation)transformation);
            throw new NotSupportedException();
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
            var crsFrom = EpsgMicroDatabase.Default.GetCrs(3857);
            var crsTo = EpsgMicroDatabase.Default.GetCrs(4326);

            var opPath = PathGenerator.Generate(crsFrom, crsTo).Single(); 
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(crsTo, crsFrom).Single();
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
            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4979);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3855);
            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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
            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4052);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(2163);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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
            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4087);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4326);
            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4326 = new GeographicCoordinate(55, 10);
            var expected4087 = new Point2(1113194.91, 6097230.31);

            AreEqual(expected4326, transformation.TransformValue(expected4087), 0.0000003);
            AreEqual(expected4087, inverse.TransformValue(expected4326), 0.004);
        }

        [Test, Ignore(NoSampleData)]
        public void m1029_equidistantCylindricalSpherical() {
            // method: 1029
            // op: 4086
            // crs: 4088 to 4047
            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4088);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4047);
            var opPath = (ICoordinateOperationCrsPathInfo)null;// PathGenerator.Generate(fromCrs, toCrs);
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = (ICoordinateOperationCrsPathInfo)null;// PathGenerator.Generate(toCrs, fromCrs);
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

        }

        [Test, Ignore(NotSupported)]
        public void m1030_geographic3DToGravityRelatedHeight_nz_geoid2009() {
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4896);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(5332);

            var forwardPaths = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(1033, x));

            var opPath = forwardPaths.Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point3, Point3>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reversePaths = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(1033, x));
            var invOpPath = reversePaths.Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(5221);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4818);
            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(5224);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(5229);
            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(5225);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(5229);
            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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
            // crs: 4802 to 4218

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4802);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4218);

            var forwardOperations = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9601, x));

            var opPath = forwardOperations.First();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reverseOperations = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(9601, x));

            var invOpPath = reverseOperations.First();
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
            // crs: 4267 to 4326

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4204);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4326);

            var forwardOps = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9603, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateTransformInfo>().Sum(x => x.Accuracy));

            var opPath = forwardOps.First();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reverseOperations = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(9603, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateOperationInverse>().Sum(x => ((EpsgCoordinateTransformInfo)x.Core).Accuracy));

            var invOpPath = reverseOperations.First();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4267 = new GeographicCoordinate(39.7391755097579, -104.984149234204);
            var expected4326 = DenverWgs84Degrees;

            AreEqual(expected4326, transformation.TransformValue(expected4267), 0.005);
            AreEqual(expected4267, inverse.TransformValue(expected4326), 0.005);
        }

        [Test, Ignore(NoUsages)]
        public void m9604_molodensky() {
            Assert.Inconclusive(NoUsages);
        }

        [Test, Ignore(NoUsages)]
        public void m9605_abridgedMolodensky() {
            Assert.Inconclusive(NoUsages);
        }

        [Test]
        public void m9606_positionVectorTransformation_geog2DDomain() {
            // method: 9606
            // crs: 4181 to 4326

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4181);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4326);

            var forwardOps = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9606, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateTransformInfo>().Sum(x => x.Accuracy));

            var opPath = forwardOps.First();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reverseOperations = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(9606, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateOperationInverse>().Sum(x => ((EpsgCoordinateTransformInfo)x.Core).Accuracy));

            var invOpPath = reverseOperations.First();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(invOpPath);

            var expected4181 = new GeographicCoordinate(49.843933, 6.128542);
            var expected4326 = new GeographicCoordinate(49.845, 6.13);

            AreEqual(expected4326, transformation.TransformValue(expected4181), 0.002);
            AreEqual(expected4181, inverse.TransformValue(expected4326), 0.002);
        }

        [Test]
        public void m9607_coordinateFrameRotation_geog2DDomain() {
            // method: 9607
            // crs: 4181 to 4326

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4181);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4326);

            var forwardOps = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9607, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateTransformInfo>().Sum(x => x.Accuracy));

            var opPath = forwardOps.First();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reverseOperations = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(9607, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateOperationInverse>().Sum(x => ((EpsgCoordinateTransformInfo)x.Core).Accuracy));

            var invOpPath = reverseOperations.First();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(5705);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(5797);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<double, double>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4903);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4230);

            var forwardOps = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9617, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateTransformInfo>().Sum(x => x.Accuracy));

            var opPath = forwardOps.First();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var input = new GeographicCoordinate(42.647992, 3.659603);
            var expectedResult = new GeographicCoordinate(42.649117, -0.026658);

            var result = transformation.TransformValue(input);

            Assert.AreEqual(expectedResult.Latitude, result.Latitude, 0.00005);
            Assert.AreEqual(expectedResult.Longitude, result.Longitude, 0.00005);
        }

        [Test]
        public void m9618_geographic2DWithHeightOffset() {
            // method: 9618
            // op: 1335
            // crs: 4301 to 4326

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4301);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4326);

            var forwardOps = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9618, x));

            var opPath = forwardOps.Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, GeographicHeightCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reverseOperations = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(9618, x));

            var invOpPath = reverseOperations.Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicHeightCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);
        }

        [Test]
        public void m9619_geographic2DOffsets() {
            // method: 9619
            // op: 1891
            // crs: 4120 to 4121

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4120);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4121);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4120 = new GeographicCoordinate(38.14349, 23.80451);
            var expected4121 = new GeographicCoordinate(38.141863, 23.804588);

            AreEqual(expected4120, transformation.TransformValue(expected4121), 0.004);
            AreEqual(expected4121, inverse.TransformValue(expected4120), 0.004);
        }

        [Test, Ignore(NotSupported)]
        public void m9620_norwayOffshoreInterpolation() {
            Assert.Ignore(NotSupported);
        }

        [Test]
        public void m9621_similarityTransformation() {
            // method: 9621
            // op: 5166
            // crs: 23031 to 25831

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(23031);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(25831);

            var forwardOps = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9621, x));

            var opPath = forwardOps.Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reverseOperations = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(9621, x));

            var invOpPath = reverseOperations.Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, Point2>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected23031 = new Point2(300000, 4500000);
            var expected25831 = new Point2(299905.060, 4499796.515);

            AreEqual(expected25831, transformation.TransformValue(expected23031), 0.001);
            AreEqual(expected23031, inverse.TransformValue(expected25831), 0.001);
        }

        [Test, Ignore(NoUsages)]
        public void m9622_affineOrthogonalGeometricTransformation() {
            Assert.Inconclusive(NoUsages);
        }

        [Test, Ignore(NoUsages)]
        public void m9623_affineGeometricTransformation() {
            Assert.Inconclusive(NoUsages);
        }

        [Test]
        public void m9624_affineParametricTransformation() {
            // method: 9624
            // op: 15857
            // crs: 3367 to 3343

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(3367);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3343);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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
            // crs: 4181 to 4326

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4181);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4326);

            var forwardOps = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9636, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateTransformInfo>().Sum(x => x.Accuracy));

            var opPath = forwardOps.First();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reverseOperations = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(9636, x))
                .OrderBy(op => op.CoordinateOperations.OfType<EpsgCoordinateOperationInverse>().Sum(x => ((EpsgCoordinateTransformInfo)x.Core).Accuracy));

            var invOpPath = reverseOperations.First();
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

        [Test, Explicit("Example is deprecated")]
        public void m9659_geographic3DTo2D() {
            // method: 9659
            // op: 15539
            // crs: 4645 to 4969

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4645);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4969);

            var forwardOps = PathGenerator.Generate(fromCrs, toCrs)
                .Where(x => FilterForMethodCode(9659, x));

            var opPath = forwardOps.Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicHeightCoordinate, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var reverseOperations = PathGenerator.Generate(toCrs, fromCrs)
                .Where(x => FilterForMethodCode(9659, x));

            var invOpPath = reverseOperations.Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(24200);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4242);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected24200 = new Point2(255966.58, 142493.51);
            var expected4242 = new GeographicCoordinate(17.932167, -76.943683);

            AreEqual(expected4242, transformation.TransformValue(expected24200), 0.000001);
            AreEqual(expected24200, inverse.TransformValue(expected4242), 0.04);
        }

        [Test]
        public void m9802_lambertConicConformal2Sp() {
            // method: 9802
            // op: 14204
            // crs: 32040 to 4267

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(32040);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4267);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected32040 = new Point2(2963503.91, 254759.80);
            var expected4267 = new GeographicCoordinate(28.5, -96);

            AreEqual(expected4267, transformation.TransformValue(expected32040), 0.000000009);
            AreEqual(expected32040, inverse.TransformValue(expected4267), 0.003);
        }

        [Test]
        public void m9803_lamberConicConformal2SpBelgium() {
            // method: 9803
            // op: 19902
            // crs: 31300 to 4313

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(31300);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4313);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(3002);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4257);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(3388);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4284);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected3388 = new Point2(165704.29, 5171848.07);
            var expected4284 = new GeographicCoordinate(53, 53);

            AreEqual(expected4284, transformation.TransformValue(expected3388), 0.00000004);
            AreEqual(expected3388, inverse.TransformValue(expected4284), 0.004);
        }

        [Test]
        public void m9806_cassiniSoldner() {
            // method: 9806
            // op: 19925
            // crs: 30200 to 4302

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(30200);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4302);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(27700);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4277);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected27700 = new Point2(577274.99, 69740.50);
            var expected4277 = new GeographicCoordinate(50.5, 0.5);

            AreEqual(expected4277, transformation.TransformValue(expected27700), 0.0000001);
            AreEqual(expected27700, inverse.TransformValue(expected4277), 0.008);
        }

        [Test]
        public void m9808_transverseMercator_sourceOrientated() {
            // method: 9808
            // op: 17529
            // crs: 2053 to 4148

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(2053);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4148);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(28992);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4289);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(5041);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4326);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
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

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(29701);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(4810);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected29701 = new Point2(188333.848, 1098841.091);
            var expected4810 = new GeographicCoordinate(-17.9886666667, 46.800381173 + 2.5969212963); // grads

            AreEqual(expected4810, transformation.TransformValue(expected29701), 0.001);
            AreEqual(expected29701, inverse.TransformValue(expected4810), 50);
        }

        [Test, Ignore(NoUsages)]
        public void m9814_swissObliqueCylindrical() {
            Assert.Inconclusive(NoUsages);
        }

        [Test]
        public void m9815_hotineObliqueMercator_variantB() {
            // method: 9815
            // op: 19958
            // crs: 4298 to 29873

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4298);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(29873);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4298 = new GeographicCoordinate(5.387254, 115.805506); // dd
            var expected29873 = new Point2(679245.73, 596562.78); // m

            AreEqual(expected29873, transformation.TransformValue(expected4298), 0.06);
            AreEqual(expected4298, inverse.TransformValue(expected29873), 0.0005);
        }

        [Test]
        public void m9816_tunisiaMiningGrid() {
            // method: 9816
            // op: 19937
            // crs: 4816 to 22300

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4816);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(22300);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4816 = new GeographicCoordinate(38.97997, 8.22437);
            var expected22300 = TunisiaMiningGrid.GridReferenceToLocation(302598);

            AreEqual(expected22300, transformation.TransformValue(expected4816), 0.0000000000002);
            AreEqual(expected4816, inverse.TransformValue(expected22300), 0.0000000000002);
        }

        [Test]
        public void m9817_lamberConicNearConformal() {
            // method: 9817
            // op: 19940
            // crs: 4227 to 22700

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4227);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(22700);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4227 = new GeographicCoordinate(37.521563, 34.13647);
            var expected22700 = new Point2(15707.96, 623165.96);

            AreEqual(expected22700, transformation.TransformValue(expected4227), 0.06);
            AreEqual(expected4227, inverse.TransformValue(expected22700), 0.0000006);
        }

        [Test, Ignore(NoSampleData)]
        public void m9818_americalPolyconic() {
            Assert.Inconclusive(NoSampleData);
        }

        [Test]
        public void m9819_krovak() {
            // method: 9819
            // op: 19952
            // crs: 4818 to 2065

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4818);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(2065);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4818 = new GeographicCoordinate(50.209012, 16.849772);
            var expected2065 = new Point2(1050538.63, 568991.00);

            AreEqual(expected2065, transformation.TransformValue(expected4818), 0.04);
            AreEqual(expected4818, inverse.TransformValue(expected2065), 0.000001);
        }

        [Test]
        public void m9820_lambertAzimuthalEqualArea_obliqueAspect() {
            // method: 9820
            // op: 19986
            // crs: 4258 to 3035

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4258);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3035);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4258 = new GeographicCoordinate(50, 5);
            var expected3035 = new Point2(3962799.45, 2999718.85);

            AreEqual(expected3035, transformation.TransformValue(expected4258), 0.004);
            AreEqual(expected4258, inverse.TransformValue(expected3035), 0.00003);
        }

        [Test, Ignore(NoSampleData)]
        public void m9821_lambertAzimuthalEqualArea_spherical() {
            Assert.Inconclusive(NoSampleData);
        }

        [Test, Ignore(NoSampleData)]
        public void m9822_albersEqualArea() {
            Assert.Inconclusive(NoSampleData);
        }

        [Test, Ignore(NoSampleData)]
        public void m9823_equidistantCylindrical_spherical() {
            Assert.Inconclusive(NoSampleData);
        }

        [Test, Ignore(NotSupported)]
        public void m9824_9824_transverseMercatorZonedGridSystem() {
            Assert.Inconclusive(NotSupported);
        }

        [Test, Ignore(NoUsages)]
        public void m9825_pseudoPlateCarree() {
            Assert.Inconclusive(NoUsages);
        }

        [Test, Ignore(NoSampleData)]
        public void m9826_lambertConicConformal_westOrientated() {
            Assert.Ignore(NoSampleData);
        }

        [Test, Ignore(NoUsages)]
        public void m9827_bonne() {
            Assert.Ignore(NoUsages);
        }

        [Test, Ignore(NoSampleData)]
        public void m9828_bonne_southOrientated() {
            Assert.Ignore(NoSampleData);
        }

        [Test]
        public void m9829_polarStereographic_variantB() {
            // method: 9829
            // op: 19993
            // crs: 4326 to 3032

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4326);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3032);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4326 = new GeographicCoordinate(-75, 120);
            var expected3032 = new Point2(7255380.79, 7053389.56);

            AreEqual(expected3032, transformation.TransformValue(expected4326), 0.004);
            AreEqual(expected4326, inverse.TransformValue(expected3032), 0.00000006);
        }

        [Test]
        public void m9830_polarStereographic_variantC() {
            // method: 9829
            // op: 19983
            // crs: 4636 to 2985

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4636);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(2985);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4636 = new GeographicCoordinate(-66.605228, 140.0714);
            var expected2985 = new Point2(303169.52, 244055.72);

            AreEqual(expected2985, transformation.TransformValue(expected4636), 0.03);
            AreEqual(expected4636, inverse.TransformValue(expected2985), 0.0000003);
        }

        [Test]
        public void m9831_guamProjection() {
            // method: 9831
            // op: 15400
            // crs: 4675 to 3993

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4675);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3993);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4675 = new GeographicCoordinate(13.3390, 144.6353);
            var expected3993 = new Point2(37712.48, 35242.00);

            AreEqual(expected3993, transformation.TransformValue(expected4675), 5);
            AreEqual(expected4675, inverse.TransformValue(expected3993), 0.05);
        }

        [Test]
        public void m9832_modifiedAzimuthalEquidistant() {
            // method: 9832
            // op: 15399
            // crs: 4675 to 3295

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4675);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3295);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4675 = new GeographicCoordinate(9.5965258333333333333333333333333, 138.19303);
            var expected3295 = new Point2(42665.90, 65509.82);

            AreEqual(expected3295, transformation.TransformValue(expected4675), 0.005);
            AreEqual(expected4675, inverse.TransformValue(expected3295), 0.00000004);
        }

        [Test]
        public void m9833_hyperbolicCassiniSoldner() {
            // method: 9833
            // op: 19878
            // crs: 4748 to 3139

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4748);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3139);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4748 = new GeographicCoordinate(-16.841456527777777777777777777778, 179.99433652777777777777777777778);
            var expected3139 = new Point2(16015.2890 * 100, 13369.6601 * 100); // test data is defined in chains but the CRS is in links

            AreEqual(expected3139, transformation.TransformValue(expected4748), 0.005);
            AreEqual(expected4748, inverse.TransformValue(expected3139), 0.000003);
        }

        [Test]
        public void m9834_lambertCylindricalEqualArea_spherical() {
            // method: 9834
            // op: 19869
            // crs: 4053 to 3410

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4053);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3410);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4053 = DenverWgs84Degrees;
            var expected3410 = new Point2(-10173682.187651999, 4649673.6115910001);

            AreEqual(expected3410, transformation.TransformValue(expected4053), 70000);
            AreEqual(expected4053, inverse.TransformValue(expected3410), 0.7);
        }

        [Test, Ignore(NotSupported)]
        public void m9835_lambertCylindricalEqualArea() {
            Assert.Inconclusive(NotSupported);
        }

        [Test]
        public void m9836_geocentricTopocentricConversions() {
            // method: 9836
            // op: 15593
            // crs: 4978 to 5820

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4978) as ICrsGeocentric;
            var toCrs = EpsgMicroDatabase.Default.GetCrs(5820);

            Assert.AreEqual(6378137, fromCrs.Datum.Spheroid.A);
            Assert.AreEqual(298.2572236, fromCrs.Datum.Spheroid.InvF, 0.00000004);

            Assert.AreEqual(0.006694380, fromCrs.Datum.Spheroid.ESquared, 0.000000001);
            Assert.AreEqual(0.006739497, fromCrs.Datum.Spheroid.ESecondSquared, 0.0000000003);
            Assert.AreEqual(6356752.314, fromCrs.Datum.Spheroid.B, 0.0003);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();

            // NOTE: due to an apparent bug in the G7-2 or EPSG database there is a difference in topocentric origin
            // this hack corrects that difference

            var targetOp = opPath.CoordinateOperations.Single() as IParameterizedCoordinateOperationInfo;
            opPath = new CoordinateOperationCrsPathInfo(
                opPath.CoordinateReferenceSystems,
                new ICoordinateOperationInfo[] { 
                    new CoordinateOperationInfo(
                        targetOp.Name,
                        new[]{
                            new NamedParameter<double>("Geocentric X of topocentric origin", 3652755.3058),
                            new NamedParameter<double>("Geocentric Y of topocentric origin", 319574.6799),
                            new NamedParameter<double>("Geocentric Z of topocentric origin", 5201547.3536)
                        },
                        targetOp.Method,
                        null,
                        targetOp.HasInverse)
                });

            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<Point3, Point3>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);

            // NOTE: this is a hack, see above
            //var inverse = CreateTyped<Point3, Point3>(StaticCompiler.Compile(invOpPath));
            var inverse = transformation.GetInverse();

            Assert.IsNotNull(inverse);

            var expected4978 = new Point3(3771793.968, 140253.342, 5124304.349);
            var expected5820 = new Point3(-189013.869, -128642.040, -4220.171);

            AreEqual(expected5820, transformation.TransformValue(expected4978), 0.001);
            AreEqual(expected4978, inverse.TransformValue(expected5820), 0.0004);
        }

        [Test]
        public void m9837_geographicTopocentricConversions() {
            // method: 9837
            // op: 15594
            // crs: 4979 to 5819

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4979);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(5819);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicHeightCoordinate, Point3>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point3, GeographicHeightCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4979 = new GeographicHeightCoordinate(53.809394444444444444444444444444, 2.12955, 73);
            var expected5819 = new Point3(-189013.869, -128642.040, -4220.171);

            AreEqual(expected5819, transformation.TransformValue(expected4979), 201);
            AreEqual(expected4979, inverse.TransformValue(expected5819), 0.00009, 201);
        }

        [Test]
        public void m9838_verticalPerspective() {
            // method: 9838
            // op: 19850
            // crs: 5819 to 5821

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(5819);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(5821);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicHeightCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var expected5819 = new GeographicHeightCoordinate(0.939151101, 0.037167659, 73);
            var expected5821 = new Point2(-188878.767, -128550.090);

            AreEqual(expected5821, transformation.TransformValue(expected5819), 20);
        }

        [Test, Ignore(NotSupported)]
        public void m9839_verticalPerspectiveOrthographic() {
            Assert.Inconclusive(NotSupported);
        }

        [Test, Ignore(NoUsages)]
        public void m9840_orthographic() {
            Assert.Inconclusive(NoUsages);
        }

        [Test]
        public void m9841_mercator_1Sp_spherical() {
            // method: 9841
            // op: 19847
            // crs: 4055 to 3785

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4055);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(3785);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4055 = new GeographicCoordinate(39.55009499999999, -104.984722);
            var expected3785 = new Point2(-11686845.794113001, 4800777.1091670003);

            AreEqual(expected3785, transformation.TransformValue(expected4055), 0.02);
            AreEqual(expected4055, inverse.TransformValue(expected3785), 0.0000002);
        }

        [Test, Explicit]
        public void m9842_equidistantCylindrical() {
            // method: 9842
            // op: 19846
            // crs: 4326 to 32663

            var fromCrs = EpsgMicroDatabase.Default.GetCrs(4326);
            var toCrs = EpsgMicroDatabase.Default.GetCrs(32663);

            var opPath = PathGenerator.Generate(fromCrs, toCrs).Single();
            Assert.IsNotNull(opPath);
            var transformation = CreateTyped<GeographicCoordinate, Point2>(StaticCompiler.Compile(opPath));
            Assert.IsNotNull(transformation);

            var invOpPath = PathGenerator.Generate(toCrs, fromCrs).Single();
            Assert.IsNotNull(invOpPath);
            var inverse = CreateTyped<Point2, GeographicCoordinate>(StaticCompiler.Compile(invOpPath));
            Assert.IsNotNull(inverse);

            var expected4326 = DenverWgs84Degrees;
            var expected32663 = new Point2(-11686845.794113001, 4423743.8349890001);

            Assert.Inconclusive("Accuracy issues.");

            AreEqual(expected32663, transformation.TransformValue(expected4326), 10);
            AreEqual(expected4326, inverse.TransformValue(expected32663), 0);
        }

        [Test, Ignore(NoUsages)]
        public void m9843_geographic2DAxisOrderReversal() {
            Assert.Inconclusive(NoUsages);
        }

        [Test, Ignore(NoUsages)]
        public void m9844_geographic3DCrsAxisOrderChange() {
            Assert.Inconclusive(NoUsages);
        }

    }

}
