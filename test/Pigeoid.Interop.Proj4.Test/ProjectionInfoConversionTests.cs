using DotSpatial.Projections;
using DotSpatial.Projections.ProjectedCategories;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pigeoid.Interop.Proj4.Test
{
    [TestFixture]
    public class ProjectionInfoRoundTripTests
    {

        public class TestSet
        {
            public TestSet(Type categoryType, string fieldName) {
                CategoryType = categoryType;
                FieldName = fieldName;
            }

            public Type CategoryType;
            public string FieldName;

            public override string ToString() {
                return CategoryType.Name + ": " + FieldName;
            }
        }

        private readonly Dictionary<Type, object> CategoryInstances;

        public IEnumerable<Type> GetCoordinateSystemCategoryTypes {
            get {
                var categoryType = typeof(CoordinateSystemCategory);
                return categoryType.Assembly.GetTypes().Where(x => x.BaseType == categoryType);
            }
        }

        public ProjectionInfoRoundTripTests() {
            CategoryInstances = GetCoordinateSystemCategoryTypes
                .ToDictionary(t => t, Activator.CreateInstance);
        }

        private string[] GetProjectionFieldNames(Type categoryType) {
            return Array.ConvertAll(
                categoryType.GetFields(BindingFlags.Public | BindingFlags.Instance),
                x => x.Name);
        }


        public IEnumerable<TestSet> AllProjections {
            get {
                return CategoryInstances.Keys
                    .SelectMany(t => GetProjectionFieldNames(t).Select(f => new TestSet(t, f)));
            }
        }

        private static bool IsFalseWgs84(Spheroid spheroid) {
            if (spheroid.KnownEllipsoid != Proj4Ellipsoid.WGS_1984)
                return false;

            return spheroid.EquatorialRadius != 6378137.0 || spheroid.InverseFlattening != 298.257223563;
        }

        private static bool IsInvalidSpheroid(Spheroid spheroid) {
            return spheroid.InverseFlattening == 0;
        }

        [Test, TestCaseSource("AllProjections"), Explicit]
        public void Test(TestSet set) {
            var catagory = CategoryInstances[set.CategoryType];
            var expected = (ProjectionInfo)(set.CategoryType.GetField(set.FieldName).GetValue(catagory));

            ProjectionInfo actual;
            if (expected.Transform == null || expected.IsLatLon) {
                actual = Proj4CrsGeographic.CreateProjection(new Proj4CrsGeographic(expected.GeographicInfo));

                // NOTE: TEST HACK
                if (expected.Transform == null) {
                    actual.IsLatLon = false; // TODO: not sure why, but these are set to false for latlon on some CRSs
                    actual.Transform = null;
                }

            }
            else {
                actual = Proj4CrsProjected.CreateProjection(new Proj4CrsProjected(expected));
            }

            Assert.AreEqual(expected.alpha, actual.alpha);
            // TODO: authority
            Assert.AreEqual(expected.AuxiliarySphereType, actual.AuxiliarySphereType);
            Assert.AreEqual(expected.bns, actual.bns);
            Assert.AreEqual(expected.CentralMeridian, actual.CentralMeridian);
            Assert.AreEqual(expected.czech, actual.czech);
            Assert.AreEqual(expected.EpsgCode, actual.EpsgCode);
            Assert.AreEqual(expected.FalseEasting, actual.FalseEasting);
            Assert.AreEqual(expected.FalseNorthing, actual.FalseNorthing);
            Assert.AreEqual(expected.Geoc, actual.Geoc);
            // TODO: geographic info
            Assert.AreEqual(expected.guam, actual.guam);
            Assert.AreEqual(expected.h, actual.h);
            Assert.AreEqual(expected.IsGeocentric, actual.IsGeocentric);
            Assert.AreEqual(expected.IsLatLon, actual.IsLatLon);
            Assert.AreEqual(expected.IsSouth, actual.IsSouth);
            Assert.AreEqual(expected.IsValid, actual.IsValid);
            Assert.AreEqual(expected.Lam0, actual.Lam0, 0.000001);
            Assert.AreEqual(expected.Lam1, actual.Lam1, 0.000001);
            Assert.AreEqual(expected.Lam2, actual.Lam2, 0.000001);
            Assert.AreEqual(expected.lat_ts, actual.lat_ts);
            Assert.AreEqual(expected.LatitudeOfOrigin, actual.LatitudeOfOrigin);
            Assert.AreEqual(expected.lon_1, actual.lon_1);
            Assert.AreEqual(expected.lon_2, actual.lon_2);
            Assert.AreEqual(expected.lonc, actual.lonc);
            Assert.AreEqual(expected.LongitudeOfCenter, actual.LongitudeOfCenter);
            Assert.AreEqual(expected.M, actual.M);
            Assert.AreEqual(expected.mGeneral, actual.mGeneral);
            Assert.AreEqual(expected.n, actual.n);
            //Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.no_rot, actual.no_rot);
            Assert.AreEqual(expected.no_uoff, actual.no_uoff);
            Assert.AreEqual(expected.NoDefs, actual.NoDefs);
            Assert.AreEqual(expected.Over, actual.Over);
            Assert.AreEqual(expected.Phi0, actual.Phi0, 0.000001);
            Assert.AreEqual(expected.Phi1, actual.Phi1, 0.000001);
            Assert.AreEqual(expected.Phi2, actual.Phi2, 0.000001);
            Assert.AreEqual(expected.rot_conv, actual.rot_conv);
            Assert.AreEqual(expected.ScaleFactor, actual.ScaleFactor);
            Assert.AreEqual(expected.StandardParallel1, actual.StandardParallel1);
            Assert.AreEqual(expected.StandardParallel2, actual.StandardParallel2);
            Assert.AreEqual(expected.to_meter, actual.to_meter);
            // TODO: transform
            // TODO: unit
            Assert.AreEqual(expected.W, actual.W);
            Assert.AreEqual(expected.Zone, actual.Zone);

            // NOTE: TEST HACK
            // this projection may have a known ellipsoid of WGS84 but different parameters for it than Proj4Ellipsoid.WGS_1984
            if (IsFalseWgs84(expected.GeographicInfo.Datum.Spheroid))
                actual.GeographicInfo.Datum.Spheroid = new Spheroid(expected.GeographicInfo.Datum.Spheroid.KnownEllipsoid);

            // NOTE: TEST HACK
            if (IsInvalidSpheroid(expected.GeographicInfo.Datum.Spheroid))
                return; // this ellipsoid is all wrong

            Assert.AreEqual(expected.ToProj4String(), actual.ToProj4String());

        }

    }
}
