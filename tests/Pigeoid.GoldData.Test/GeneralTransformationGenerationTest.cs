using NUnit.Framework;
using Pigeoid.CoordinateOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.GoldData.Test
{

    [TestFixture]
    public class GeneralTransformationGenerationTest
    {

        [Test]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_14.csv", 0.0000005, 0.0000000002)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_14.csv", 0.000001, 0.00000000011)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_14a.csv", 0.000001, 0.00000000011)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_15.csv", 0.000001, 0.00000000011)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_21.csv", 0.0035, 0.000000004)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_22.csv", 0.0005, 0.000000004)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_23.csv", 0.000001, 0.00000000011)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_16.csv", 0.0000006, 0.000000000066)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_16a.csv", 0.0000006, 0.000000000071)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_17.csv", 0.0000006, 0.00000000002)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_18.csv", 0.00015, 1.3948895055401E-09)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_19.csv", 0.00000051, 0.000000000017)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_20.csv", 0.000006, 0.00000000005)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_5.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_5a.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_6.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_7.csv", 0.000002, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_8.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_8a.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.Mercator_8b.csv", 0.0000005, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_09.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_09a.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_09b.csv", 0.0000006, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_10.csv", 0.000001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_11.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_11a.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_11b.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_12.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_12a.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_13.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.PolarStereo_13a.csv", 0.00001, 0.000000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.TransMerc_26.csv", 0.009, 0.0000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.TransMerc_26a.csv", 0.009, 0.0000001)]
        [TestCase("Wgs84.Lat_Lon.csv", "Wgs84.TransMerc_27.csv", 0.04, 0.0000001)]
        public void Test(string sourceResourceName, string targetResourceName, double projectDelta, double unprojectDelta) {

            var sourceData = GoldData.GetReadyReader(sourceResourceName);
            var targetData = GoldData.GetReadyReader(targetResourceName);
            var sourceCrs = GoldData.GetCrs(sourceData);
            var targetCrs = GoldData.GetCrs(targetData);

            Assert.IsNotNull(sourceCrs);
            Assert.IsNotNull(targetCrs);

            var operationGenerator = new HelmertCrsCoordinateOperationPathGenerator();
            var operationPath = operationGenerator.Generate(sourceCrs, targetCrs).First();
            Assert.IsNotNull(operationPath);
            var compiler = new StaticCoordinateOperationCompiler();
            var forward = new CompiledConcatenatedTransformation<GeographicCoordinate, Point2>(compiler.Compile(operationPath) as IEnumerable<ITransformation>);
            var inverse = forward.GetInverse();

            while (sourceData.Read() && targetData.Read()) {
                var geogreaphicExpected = sourceData.CurrentLatLon();
                var projectedExpected = targetData.CurrentPoint2D();

                var projected = forward.TransformValue(geogreaphicExpected);
                Assert.AreEqual(projectedExpected.X, projected.X, projectDelta);
                Assert.AreEqual(projectedExpected.Y, projected.Y, projectDelta);

                var geographic = inverse.TransformValue(projectedExpected);
                Assert.AreEqual(geogreaphicExpected.Latitude, geographic.Latitude, unprojectDelta);
                Assert.AreEqual(geogreaphicExpected.Longitude, geographic.Longitude, unprojectDelta);
            }

        }

    }

}
