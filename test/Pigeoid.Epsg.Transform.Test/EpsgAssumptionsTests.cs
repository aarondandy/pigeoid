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
        public void all_geocentric_transforms_are_to_and_from_geocentric() {
            var txs = EpsgMicroDatabase.Default.GetCoordinateTransformInfos()
                .Where(x => x.SourceCrs.Kind == EpsgCrsKind.Geocentric || x.TargetCrs.Kind == EpsgCrsKind.Geocentric);
            foreach (var op in txs) {
                Assert.AreEqual(EpsgCrsKind.Geocentric, op.SourceCrs.Kind);
                Assert.AreEqual(EpsgCrsKind.Geocentric, op.TargetCrs.Kind);
            }
        }

        [Test]
        public void all_engineerings_transforms_are_between_projected_and_engineering() {
            var txs = EpsgMicroDatabase.Default.GetCoordinateTransformInfos()
                .Where(x => x.SourceCrs.Kind == EpsgCrsKind.Engineering || x.TargetCrs.Kind == EpsgCrsKind.Engineering);
            foreach (var op in txs) {
                Assert.That(op.SourceCrs.Kind == EpsgCrsKind.Engineering || op.SourceCrs.Kind == EpsgCrsKind.Projected);
                Assert.That(op.TargetCrs.Kind == EpsgCrsKind.Engineering || op.TargetCrs.Kind == EpsgCrsKind.Projected);
            }
        }

        [Test]
        public void all_crs_with_base_move_towards_geocentric_as_expected() {
            var allCrsWithBaseOp = EpsgMicroDatabase.Default.GetAllNormalCrs()
                .OfType<EpsgCrsGeodetic>()
                .Where(x => x.HasBaseOperationCode);
            foreach (var crs in allCrsWithBaseOp) {
                var opCode = crs.BaseOperationCode;
                var op = EpsgMicroDatabase.Default.GetCoordinateConversionInfo(opCode);
                Assert.IsNotNull(op);
                var crsKind = crs.Kind;
                var baseKind = crs.BaseCrs.Kind;
                switch (crsKind) {
                    case EpsgCrsKind.Projected:
                        Assert.AreNotEqual(EpsgCrsKind.Engineering, baseKind);
                        break;
                    case EpsgCrsKind.Geographic2D:
                        Assert.AreNotEqual(EpsgCrsKind.Projected, baseKind);
                        Assert.AreNotEqual(EpsgCrsKind.Engineering, baseKind);
                        break;
                    case EpsgCrsKind.Geographic3D:
                        Assert.AreNotEqual(EpsgCrsKind.Projected, baseKind);
                        Assert.AreNotEqual(EpsgCrsKind.Engineering, baseKind);
                        Assert.AreNotEqual(EpsgCrsKind.Geographic2D, baseKind);
                        break;
                    case EpsgCrsKind.Geocentric:
                        throw new InvalidOperationException("Geocentric should not have a base.");
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        [Test]
        public void all_cat_steps_are_non_null() {
            var allCatOps = EpsgMicroDatabase.Default.GetConcatenatedCoordinateOperationInfos();
            foreach (var catOp in allCatOps) {
                foreach (var step in catOp.Steps) {
                    Assert.IsNotNull(step);
                }
            }
        }

        [Test]
        public void no_cat_ops_for_geocentric() {
            var catOps = EpsgMicroDatabase.Default.GetConcatenatedCoordinateOperationInfos();
            foreach (var op in catOps) {
                Assert.AreNotEqual(EpsgCrsKind.Geocentric, op.SourceCrs.Kind);
                Assert.AreNotEqual(EpsgCrsKind.Geocentric, op.TargetCrs.Kind);
            }
        }

        [Test]
        public void no_cat_ops_for_engineering() {
            var catOps = EpsgMicroDatabase.Default.GetConcatenatedCoordinateOperationInfos();
            foreach (var op in catOps) {
                Assert.AreNotEqual(EpsgCrsKind.Engineering, op.SourceCrs.Kind);
                Assert.AreNotEqual(EpsgCrsKind.Engineering, op.TargetCrs.Kind);
            }
        }

        [Test]
        public void no_cat_ops_for_vertical() {
            var catOps = EpsgMicroDatabase.Default.GetConcatenatedCoordinateOperationInfos();
            foreach (var op in catOps) {
                Assert.AreNotEqual(EpsgCrsKind.Vertical, op.SourceCrs.Kind);
                Assert.AreNotEqual(EpsgCrsKind.Vertical, op.TargetCrs.Kind);
            }
        }

        [Test]
        public void no_cat_ops_for_compound() {
            var catOps = EpsgMicroDatabase.Default.GetConcatenatedCoordinateOperationInfos();
            foreach (var op in catOps) {
                Assert.AreNotEqual(EpsgCrsKind.Compound, op.SourceCrs.Kind);
                Assert.AreNotEqual(EpsgCrsKind.Compound, op.TargetCrs.Kind);
            }
        }

        [Test]
        public void all_transforms_transition_as_expected() {
            var txs = EpsgMicroDatabase.Default.GetCoordinateTransformInfos();
            var catOps = EpsgMicroDatabase.Default.GetConcatenatedCoordinateOperationInfos();

            Assert.Inconclusive();
        }

    }
}
