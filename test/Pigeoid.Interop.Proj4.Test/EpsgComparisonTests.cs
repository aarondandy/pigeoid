using DotSpatial.Projections;
using DotSpatial.Projections.ProjectedCategories;
using NUnit.Framework;
using Pigeoid.Epsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Interop.Proj4.Test
{
    [TestFixture]
    public class EpsgComparisonTests
    {

        [Test]
        public void epsg2039() {
            var crs = EpsgCrs.Get(2039);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(31.73439361111111, prj.LatitudeOfOrigin, 0.000000001);
            Assert.AreEqual(35.20451694444445, prj.CentralMeridian, 0.000000001);
            Assert.AreEqual(1.0000067, prj.ScaleFactor);
            Assert.AreEqual(219529.584, prj.FalseEasting);
            Assert.AreEqual(626907.39, prj.FalseNorthing);

            Assert.AreEqual(-48, prj.GeographicInfo.Datum.ToWGS84[0]);
            Assert.AreEqual(55, prj.GeographicInfo.Datum.ToWGS84[1]);
            Assert.AreEqual(52, prj.GeographicInfo.Datum.ToWGS84[2]);
        }

        [Test]
        public void epsg2281() {
            var crs = EpsgCrs.Get(2281);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(40.65, prj.StandardParallel1);
            Assert.AreEqual(39.01666666666667, prj.StandardParallel2, 0.0000000001);
            Assert.AreEqual(-111.5, prj.CentralMeridian);
            Assert.AreEqual(500000, prj.FalseEasting, 0.001);
            Assert.AreEqual(2000000, prj.FalseNorthing, 0.001);
        }

        [Test]
        public void epsg2318() {
            var crs = EpsgCrs.Get(2318);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(17, prj.StandardParallel1);
            Assert.AreEqual(33, prj.StandardParallel2);
            Assert.AreEqual(25.08951, prj.LatitudeOfOrigin, 0.0001);
            Assert.AreEqual(48, prj.CentralMeridian);
            Assert.AreEqual(0, prj.FalseEasting);
            Assert.AreEqual(0, prj.FalseNorthing);

        }

        [Test]
        public void epsg2469() {
            var crs = EpsgCrs.Get(2469);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(0, prj.LatitudeOfOrigin);
            Assert.AreEqual(57, prj.CentralMeridian);
            Assert.AreEqual(1, prj.ScaleFactor);
            Assert.AreEqual(500000, prj.FalseEasting);
            Assert.AreEqual(0, prj.FalseNorthing);
        }

        [Test]
        public void epsg2985() {
            var crs = EpsgCrs.Get(2985);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual("stere", prj.Transform.Proj4Name);
            Assert.AreEqual(-90, prj.LatitudeOfOrigin);
            Assert.AreEqual(-67, prj.StandardParallel1);
            Assert.AreEqual(140, prj.CentralMeridian);
            Assert.AreEqual(1, prj.ScaleFactor);
            Assert.AreEqual(300000, prj.FalseEasting);
            Assert.AreEqual(200000, prj.FalseNorthing);
        }

        [Test]
        public void epsg3031() {
            var crs = EpsgCrs.Get(3031);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual("stere", prj.Transform.Proj4Name);
            Assert.AreEqual(-90, prj.LatitudeOfOrigin);
            Assert.AreEqual(-71, prj.StandardParallel1);
            Assert.AreEqual(0, prj.CentralMeridian ?? 0);
            Assert.AreEqual(1, prj.ScaleFactor);
            Assert.AreEqual(0, prj.FalseEasting);
            Assert.AreEqual(0, prj.FalseNorthing);
        }

        [Test]
        public void epsg3078() {
            var crs = EpsgCrs.Get(3078);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(45.30916666666666, prj.LatitudeOfOrigin, 0.00000001);
            Assert.AreEqual(-86, prj.LongitudeOfCenter);
            Assert.AreEqual(337.25556, prj.alpha);
            Assert.AreEqual(0.9996, prj.ScaleFactor);
            Assert.AreEqual(2546731.496, prj.FalseEasting);
            Assert.AreEqual(-4354009.816, prj.FalseNorthing);

            Assert.AreEqual(6378137, prj.GeographicInfo.Datum.Spheroid.EquatorialRadius, 0.001);
            Assert.AreEqual(298.257222101, prj.GeographicInfo.Datum.Spheroid.InverseFlattening, 0.001);
        }

        [Test]
        public void epsg3079() {
            var crs = EpsgCrs.Get(3079);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(45.30916666666666, prj.LatitudeOfOrigin, 0.00000001);
            Assert.AreEqual(-86, prj.LongitudeOfCenter);
            Assert.AreEqual(0.9996, prj.ScaleFactor);
            Assert.AreEqual(2546731.496, prj.FalseEasting);
            Assert.AreEqual(-4354009.816, prj.FalseNorthing);
        }

        [Test]
        public void epsg3293() {
            var crs = EpsgCrs.Get(3293);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual("stere", prj.Transform.Proj4Name);
            Assert.AreEqual(-90, prj.LatitudeOfOrigin);
            Assert.AreEqual(-80.23861111111111, prj.StandardParallel1, 0.0000001);
            Assert.AreEqual(0, prj.LongitudeOfCenter ?? 0);
            Assert.AreEqual(1, prj.ScaleFactor);
            Assert.AreEqual(0, prj.FalseEasting);
            Assert.AreEqual(0, prj.FalseNorthing);
        }

        [Test]
        public void epsg3571() {
            var crs = EpsgCrs.Get(3571);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual("laea", prj.Transform.Proj4Name);
            Assert.AreEqual(90, prj.LatitudeOfOrigin);
            Assert.AreEqual(180, prj.CentralMeridian);
            Assert.AreEqual(0, prj.FalseEasting);
            Assert.AreEqual(0, prj.FalseNorthing);
        }

        [Test]
        public void epsg3574() {
            var crs = EpsgCrs.Get(3574);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(90, prj.LatitudeOfOrigin);
            Assert.AreEqual(-40, prj.CentralMeridian);
            Assert.AreEqual(0, prj.FalseEasting);
            Assert.AreEqual(0, prj.FalseNorthing);
        }

        [Test]
        public void epsg3575() {
            var crs = EpsgCrs.Get(3575);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual("laea", prj.Transform.Proj4Name);
            Assert.AreEqual(90, prj.LatitudeOfOrigin);
            Assert.AreEqual(10, prj.CentralMeridian);
            Assert.AreEqual(0, prj.FalseEasting);
            Assert.AreEqual(0, prj.FalseNorthing);
        }

        [Test]
        public void epsg26931() {
            var crs = EpsgCrs.Get(26931);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(57, prj.LatitudeOfOrigin);
            Assert.AreEqual(-133.6666666666667, prj.LongitudeOfCenter, 0.000000001);
            Assert.AreEqual(323.1301023611111, prj.alpha, 0.000000001);
            Assert.AreEqual(0.9999, prj.ScaleFactor, 0.000000001);
            Assert.AreEqual(5000000, prj.FalseEasting, 0.000000001);
            Assert.AreEqual(-5000000, prj.FalseNorthing, 0.000000001);
        }

        [Test]
        public void epsg28191() {
            var crs = EpsgCrs.Get(28191);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(31.73409694444445, prj.LatitudeOfOrigin, 0.000000001);
            Assert.AreEqual(35.21208055555556, prj.CentralMeridian, 0.000000001);
            Assert.AreEqual(170251.555, prj.FalseEasting);
            Assert.AreEqual(126867.909, prj.FalseNorthing);

            Assert.AreEqual(-275.722, prj.GeographicInfo.Datum.ToWGS84[0], 0.001);
            Assert.AreEqual(94.7824, prj.GeographicInfo.Datum.ToWGS84[1], 0.001);
            Assert.AreEqual(340.894, prj.GeographicInfo.Datum.ToWGS84[2], 0.001);
            Assert.AreEqual(-8.001, prj.GeographicInfo.Datum.ToWGS84[3], 0.001);
            Assert.AreEqual(-4.42, prj.GeographicInfo.Datum.ToWGS84[4], 0.001);
            Assert.AreEqual(-11.821, prj.GeographicInfo.Datum.ToWGS84[5], 0.001);
            Assert.AreEqual(1, prj.GeographicInfo.Datum.ToWGS84[6]);

            Assert.AreEqual(6378300.789, prj.GeographicInfo.Datum.Spheroid.EquatorialRadius, 0.001);
            Assert.AreEqual(6356566.435, prj.GeographicInfo.Datum.Spheroid.PolarRadius, 0.001);
        }

        [Test]
        public void epsg28192() {
            var crs = EpsgCrs.Get(28192);
            Assert.IsNotNull(crs);

            var prj = Proj4Crs.CreateProjection(crs);
            Assert.IsNotNull(prj);
            Assert.AreEqual(31.73409694444445, prj.LatitudeOfOrigin, 0.000000001);
            Assert.AreEqual(35.21208055555556, prj.CentralMeridian, 0.000000001);
            Assert.AreEqual(170251.555, prj.FalseEasting);
            Assert.AreEqual(1126867.909, prj.FalseNorthing);

            Assert.AreEqual(-275.722, prj.GeographicInfo.Datum.ToWGS84[0],0.001);
            Assert.AreEqual(94.7824, prj.GeographicInfo.Datum.ToWGS84[1], 0.001);
            Assert.AreEqual(340.894, prj.GeographicInfo.Datum.ToWGS84[2], 0.001);
            Assert.AreEqual(-8.001, prj.GeographicInfo.Datum.ToWGS84[3], 0.001);
            Assert.AreEqual(-4.42, prj.GeographicInfo.Datum.ToWGS84[4], 0.001);
            Assert.AreEqual(-11.821, prj.GeographicInfo.Datum.ToWGS84[5], 0.001);
            Assert.AreEqual(1, prj.GeographicInfo.Datum.ToWGS84[6]);

            Assert.AreEqual(6378300.789, prj.GeographicInfo.Datum.Spheroid.EquatorialRadius, 0.001);
            Assert.AreEqual(6356566.435, prj.GeographicInfo.Datum.Spheroid.PolarRadius, 0.001);
        }

        [Test]
        public void epsg2039_to_epsg28191() {
            var from = EpsgCrs.Get(2039);
            var to = EpsgCrs.Get(28191);
            /*var epsgPathGen = new EpsgCrsCoordinateOperationPathGeneratorOld(
                new EpsgCrsCoordinateOperationPathGeneratorOld.SharedOptionsAreaPredicate(
                    x => !x.Deprecated,
                    x => !x.Deprecated));*/
            var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
            var epsgPath = pathGenerator.Generate(from, to);
            Assert.IsNotEmpty(epsgPath);
            var proj4Transform = new Proj4Transform(from, to);
            Assert.IsNotNull(proj4Transform);
        }

        [Test]
        public void epsg3078_to_epsg3575() {
            var from = EpsgCrs.Get(3078);
            var to = EpsgCrs.Get(3575);
            /*var epsgPathGen = new EpsgCrsCoordinateOperationPathGeneratorOld(
                new EpsgCrsCoordinateOperationPathGeneratorOld.SharedOptionsAreaPredicate(
                    x => !x.Deprecated,
                    x => !x.Deprecated));*/
            var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
            var epsgPath = pathGenerator.Generate(from, to);
            Assert.IsNotEmpty(epsgPath);
            var proj4Transform = new Proj4Transform(from, to);
            Assert.IsNotNull(proj4Transform);
        }

        [Test]
        public void epsg3031_to_epsg3293() {
            var from = EpsgCrs.Get(3031);
            var to = EpsgCrs.Get(3293);
            var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
            var epsgPath = pathGenerator.Generate(from, to);
            Assert.IsNotEmpty(epsgPath);
            var proj4Transform = new Proj4Transform(from, to);
            Assert.IsNotNull(proj4Transform);
        }

    }
}
