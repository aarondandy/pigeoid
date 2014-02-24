using System.Linq;
using NUnit.Framework;
using Vertesaur;

namespace Pigeoid.Epsg.Transform.Test
{

    [TestFixture]
    public class EpsgHelmertDatumShiftTests
    {

        [Test]
        public void self_transform() {
            var wgs84Datum = EpsgDatumGeodetic.Get(6326) as EpsgDatumGeodetic;

            Assert.IsNotNull(wgs84Datum);
            Assert.IsTrue(wgs84Datum.IsTransformableToWgs84);
            Assert.IsNotNull(wgs84Datum.BasicWgs84Transformation);
            Assert.AreEqual(Vector3.Zero, wgs84Datum.BasicWgs84Transformation.Delta);
            Assert.AreEqual(Vector3.Zero, wgs84Datum.BasicWgs84Transformation.RotationArcSeconds);
            Assert.AreEqual(Vector3.Zero, wgs84Datum.BasicWgs84Transformation.RotationRadians);
            Assert.AreEqual(0, wgs84Datum.BasicWgs84Transformation.ScaleDeltaPartsPerMillion);
        }

        [Test]
        public void has_transform() {
            var testDatums = new[] {
                // 6746,
                6326,
                6300,
                6277,
                6281,
                6289,
                6231,
                6322,
                6324,
                6284,
                6230,
                6200,
                6272,
                6269,
                6312,
                6204,
                6319,
                6242,
                6203,
                6740,
                6284,
                6130,
                6121,
                6223,
                6264,
                6318,
                6319,
                6678,
                6263,
                6675,
                6141,
                6230,
                6672
            }.Distinct().OrderBy(x => x).ToArray();

            foreach (var datumCode in testDatums) {
                var datum = EpsgDatumGeodetic.Get(datumCode) as EpsgDatumGeodetic;
                Assert.IsNotNull(datum);
                Assert.IsTrue(datum.IsTransformableToWgs84, "No path found for " + datum.Code);
                Assert.IsNotNull(datum.BasicWgs84Transformation);
            }
        }

        [Test]
        public void proj4_ire65() {
            // 4300 to 4326 (6300) // 482.53,-130569.0 / 1000.0,564.557,-1.042,-0.214,-0.631,8.15 // ire65
            var datum = EpsgDatum.Get(6300) as EpsgDatumGeodetic;
            Assert.IsNotNull(datum);
            Assert.IsTrue(datum.IsTransformableToWgs84);
            var tx = datum.BasicWgs84Transformation;
            Assert.IsNotNull(tx);
            Assert.AreEqual(482.53, tx.Delta.X, 0.03);
            Assert.AreEqual(-130569.0/1000.0, tx.Delta.Y, 0.04);
            Assert.AreEqual(564.557, tx.Delta.Z, 0.05);
            Assert.AreEqual(new Vector3(-1.042, -0.214, -0.631), tx.RotationArcSeconds);
            Assert.AreEqual(8.15, tx.ScaleDeltaPartsPerMillion);
        }

        [Test]
        public void proj4_osgb36() {
            // 4277 to 4326 (6277) // 446.448,-125.157,542.06,0.1502,0.247,0.8421,-20.4894 // osgb36
            var datum = EpsgDatum.Get(6277) as EpsgDatumGeodetic;
            Assert.IsNotNull(datum);
            Assert.IsTrue(datum.IsTransformableToWgs84);
            var tx = datum.BasicWgs84Transformation;
            Assert.IsNotNull(tx);
            Assert.AreEqual(new Vector3(446.448, -125.157, 542.06), tx.Delta);
            //Assert.AreEqual(new Vector3(0.1502, 0.247, 0.8421), tx.RotationArcSeconds);
            Assert.AreEqual(0.1502, tx.RotationArcSeconds.X, 0.02);
            Assert.AreEqual(0.247, tx.RotationArcSeconds.Y);
            Assert.AreEqual(0.8421, tx.RotationArcSeconds.Z, 0.0001);
            Assert.AreEqual(-20.4894, tx.ScaleDeltaPartsPerMillion, 0.0004);
        }

        [Test]
        public void proj4_nzgd49() {
            // 4272 to 4326 (6272) // 59.47,-5.04,187.44,0.47,-0.1,1.024,-4.5993 // nzgd49
            var datum = EpsgDatum.Get(6272) as EpsgDatumGeodetic;
            Assert.IsNotNull(datum);
            Assert.IsTrue(datum.IsTransformableToWgs84);
            var tx = datum.BasicWgs84Transformation;
            Assert.IsNotNull(tx);
            Assert.AreEqual(new Vector3(59.47, -5.04, 187.44), tx.Delta);
            Assert.AreEqual(new Vector3(0.47, -0.1, 1.024), tx.RotationArcSeconds);
            Assert.AreEqual(-4.5993, tx.ScaleDeltaPartsPerMillion);
        }

        [Test]
        public void proj4_NAD83() {
            // 4269 to 4326 (6269) // 0,0,0 // NAD83
            var datum = EpsgDatum.Get(6269) as EpsgDatumGeodetic;
            Assert.IsNotNull(datum);
            Assert.IsTrue(datum.IsTransformableToWgs84);
            var tx = datum.BasicWgs84Transformation;
            Assert.IsNotNull(tx);
            Assert.AreEqual(Vector3.Zero, tx.Delta);
            Assert.AreEqual(Vector3.Zero, tx.RotationArcSeconds);
            Assert.AreEqual(Vector3.Zero, tx.RotationRadians);
            Assert.AreEqual(0, tx.ScaleDeltaPartsPerMillion);
            Assert.AreEqual(1, tx.ScaleFactor);
        }

        [Test]
        public void proj4_hermannskogel() {
            // I could not find a matching EPSG datum
            // 4312 to 4326 (6312) // 653.0,-212.0,449.0 // hermannskogel (overriden by 7 param?)
            var datum = EpsgDatum.Get(6312) as EpsgDatumGeodetic;
            Assert.IsNotNull(datum);
            Assert.IsTrue(datum.IsTransformableToWgs84);
            var tx = datum.BasicWgs84Transformation;
            Assert.IsNotNull(tx);
            Assert.Inconclusive("I'm not sure if this is the right datum for hermannskogel or if Proj4 differs from EPSG.");
        }

        [Test]
        public void proj4_potsdam() {
            // 4746 to 4326 (6746) // 606.0,23.0,413.0 // potsdam
            var datum = EpsgDatum.Get(6746) as EpsgDatumGeodetic;
            Assert.IsNotNull(datum);
            Assert.Inconclusive("I don't see a way to find this within the EPSG data set. Is it an approximation of NTv2?");
        }

        [Test]
        public void proj4_GGRS87() {
            // 4121 to 4326 (6121) // -199.87,74.79,246.62 // GGRS87
            var datum = EpsgDatum.Get(6121) as EpsgDatumGeodetic;
            Assert.IsNotNull(datum);
            Assert.IsTrue(datum.IsTransformableToWgs84);
            var tx = datum.BasicWgs84Transformation;
            Assert.IsNotNull(tx);
            Assert.AreEqual(new Vector3(-199.87, 74.79, 246.62), tx.Delta);
            Assert.AreEqual(Vector3.Zero, tx.RotationArcSeconds);
            Assert.AreEqual(Vector3.Zero, tx.RotationRadians);
            Assert.AreEqual(0, tx.ScaleDeltaPartsPerMillion);
            Assert.AreEqual(1, tx.ScaleFactor);
        }

        [Test]
        public void proj4_carthage() {
            // 4223 to 4326 (6223) // -263.0,6.0,413.0 // carthage
            var datum = EpsgDatum.Get(6223) as EpsgDatumGeodetic;
            Assert.IsNotNull(datum);
            Assert.IsTrue(datum.IsTransformableToWgs84);
            var tx = datum.BasicWgs84Transformation;
            Assert.IsNotNull(tx);
            Assert.AreEqual(-263.0, tx.Delta.X, 3);
            Assert.AreEqual(6.0, tx.Delta.Y, 0.5);
            Assert.AreEqual(413.0, tx.Delta.Z, 20);
            Assert.AreEqual(Vector3.Zero, tx.RotationArcSeconds);
            Assert.AreEqual(Vector3.Zero, tx.RotationRadians);
            Assert.AreEqual(0, tx.ScaleDeltaPartsPerMillion);
            Assert.AreEqual(1, tx.ScaleFactor);
        }

    }
}
