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
            var generator = new EpsgCrsCoordinateOperationPathGenerator();
            var crs4267 = EpsgMicroDatabase.Default.GetCrs(4267);
            var crs4326 = EpsgMicroDatabase.Default.GetCrs(4326);

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

    }
}
