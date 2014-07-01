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

        [Test, TestCaseSource("AllProjections")]
        public void Test(TestSet set) {
            var catagory = CategoryInstances[set.CategoryType];
            var expected = (ProjectionInfo)(set.CategoryType.GetField(set.FieldName).GetValue(catagory));

            if (expected.Transform == null) {
                var actual = Proj4CrsGeographic.CreateGeographic(new Proj4CrsGeographic(expected.GeographicInfo));
                Assert.AreEqual(expected.ToProj4String(), actual.ToProj4String());
            }
            else {
                var actual = Proj4CrsProjected.CreateProjection(new Proj4CrsProjected(expected));
                Assert.AreEqual(expected.ToProj4String(), actual.ToProj4String());
            }
        }

    }
}
