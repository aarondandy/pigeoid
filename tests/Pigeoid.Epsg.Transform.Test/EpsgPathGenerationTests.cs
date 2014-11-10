using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.Transform.Test
{
    [TestFixture]
    public class EpsgPathGenerationTests
    {

        [Test]
        public void has_cat_paths_4267_and_4326() {
            var crs4267 = EpsgMicroDatabase.Default.GetCrs(4267);
            var crs4326 = EpsgMicroDatabase.Default.GetCrs(4326);

            var generator = new EpsgCrsCoordinateOperationPathGenerator();
            var forwardPaths = generator.Generate(crs4267, crs4326);
            var forwardWithCatOps = forwardPaths
                .Select(x => x.CoordinateOperations)
                .Where(x => x.OfType<EpsgConcatenatedCoordinateOperationInfo>().Any());
            Assert.IsNotEmpty(forwardWithCatOps);

            var inversePaths = generator.Generate(crs4326, crs4267);
            var inverseWithCatOps = inversePaths
                .Select(x => x.CoordinateOperations)
                .Where(x => x
                    .OfType<EpsgCoordinateOperationInverse>()
                    .Select(y => y.Core)
                    .OfType<EpsgConcatenatedCoordinateOperationInfo>()
                    .Any());
            Assert.IsNotEmpty(inverseWithCatOps);
        }

        [Test]
        public void old_web_mercator_to_new_web_mercator() {
            var oldCrs = EpsgMicroDatabase.Default.GetCrs(3785);
            var newCrs = EpsgMicroDatabase.Default.GetCrs(3857);

            var generator = new EpsgCrsCoordinateOperationPathGenerator();
            var forwardPaths = generator.Generate(oldCrs, newCrs);
            Assert.IsNotEmpty(forwardPaths);
        }

        [Test]
        public void old_web_mercator_to_nad83() {
            var oldCrs = EpsgMicroDatabase.Default.GetCrs(3785);
            var newCrs = EpsgMicroDatabase.Default.GetCrs(4269);

            var generator = new EpsgCrsCoordinateOperationPathGenerator();
            var forwardPaths = generator.Generate(oldCrs, newCrs);
            Assert.IsNotEmpty(forwardPaths);
        }

    }
}
