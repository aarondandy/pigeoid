using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.Transform.Test
{
    [TestFixture]
    public class EpsgAssumptionsTests
    {

        [Test]
        public void all_geocentric_transforms_are_to_geocentric() {
            var geocTxs = EpsgCoordinateOperationInfoRepository.TransformInfos
                .Where(x => x.SourceCrs.Kind == EpsgCrsKind.Geocentric || x.TargetCrs.Kind == EpsgCrsKind.Geocentric);
            foreach (var tx in geocTxs) {
                Assert.AreEqual(EpsgCrsKind.Geocentric, tx.SourceCrs.Kind);
                Assert.AreEqual(EpsgCrsKind.Geocentric, tx.TargetCrs.Kind);
            }
        }


    }
}
