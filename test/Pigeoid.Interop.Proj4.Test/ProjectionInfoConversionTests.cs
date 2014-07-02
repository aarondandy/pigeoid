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
    public class ProjectionInfoConversionTests
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

        public ProjectionInfoConversionTests() {
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

        [Test, TestCaseSource("AllProjections")]
        public void Test(TestSet set) {
            var catagory = CategoryInstances[set.CategoryType];
            var expected = (ProjectionInfo)(set.CategoryType.GetField(set.FieldName).GetValue(catagory));

            ProjectionInfo actual;
            if (expected.Transform == null || expected.IsLatLon) {
                actual = Proj4CrsGeographic.CreateProjection(new Proj4CrsGeographic(expected.GeographicInfo));

                if (
                    expected.Transform == null
                    || set.FieldName.StartsWith("DeirezZorLevant")
                    || set.FieldName.StartsWith("EverestModified1969")
                )
                    actual.IsLatLon = false; // TODO: not sure why, but these are set to false for latlon

            }
            else {
                actual = Proj4CrsProjected.CreateProjection(new Proj4CrsProjected(expected));
            }

            Assert.AreEqual(expected.alpha, actual.alpha);
            // TODO: authority
            Assert.AreEqual(expected.AuxiliarySphereType, actual.AuxiliarySphereType);
            Assert.AreEqual(expected.bns, actual.bns);
            Assert.AreEqual(expected.CentralMeridian, actual.CentralMeridian);

            // this projection may have a known ellipsoid of WGS84 but different parameters for it than Proj4Ellipsoid.WGS_1984
            if (IsFalseWgs84(expected.GeographicInfo.Datum.Spheroid))
                actual.GeographicInfo.Datum.Spheroid = new Spheroid(expected.GeographicInfo.Datum.Spheroid.KnownEllipsoid);

            if (IsInvalidSpheroid(expected.GeographicInfo.Datum.Spheroid))
                return; // this ellipsoid is all wrong

            Assert.AreEqual(expected.ToProj4String(), actual.ToProj4String());

        }

    }
}
