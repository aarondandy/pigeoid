using Pigeoid.CoordinateOperation;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    public class EpsgCrsGraphSearcher
    {

        private class PathNode
        {

            public static PathNode CreateStartNode(EpsgCrs crs) {
                Contract.Requires(crs != null);
                Contract.Ensures(Contract.Result<PathNode>() != null);
                return new PathNode(crs);
            }

            private PathNode(EpsgCrs crs) {
                Contract.Requires(crs != null);
            }

            [ContractInvariantMethod]
            private void ObjectInvariants() {
                Contract.Invariant(Crs != null);
            }

            public PathNode Parent;
            public EpsgCoordinateOperationInfoBase EdgeFromParent;
            public EpsgCrs Crs;

            public bool HasUsedEdge(int edgeCode) {
                var pathNode = this;
                do {
                    var edge = pathNode.EdgeFromParent;
                    if (edge == null) {
                        Contract.Assume(pathNode.Parent == null);
                        return false;
                    }
                    if (edge.Code == edgeCode)
                        return true;

                    pathNode = pathNode.Parent;
                    Contract.Assume(pathNode != null); // because pathNode.EdgeFromParent was checked
                } while (true); // pathNode != null
            }
        }

        public EpsgCrsGraphSearcher(EpsgCrs sourceCrs, EpsgCrs targetCrs) {
            if (sourceCrs == null) throw new ArgumentNullException("sourceCrs");
            if (targetCrs == null) throw new ArgumentNullException("targetCrs");
            Contract.EndContractBlock();
            SourceCrs = sourceCrs;
            SourceCrsCode = sourceCrs.Code;
            TargetCrs = targetCrs;
            TargetCrsCode = targetCrs.Code;
        }

        public EpsgCrs SourceCrs { get; private set; }

        public EpsgCrs TargetCrs { get; private set; }

        private readonly int SourceCrsCode;

        private readonly int TargetCrsCode;

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(SourceCrs != null);
            Contract.Invariant(TargetCrs != null);
        }

        public IEnumerable<ICoordinateOperationCrsPathInfo> FindAllPaths() {
            var rootNode = PathNode.CreateStartNode(SourceCrs);
            var allPaths = FindAllPathsFrom(rootNode);
            throw new NotImplementedException();
        }

        private IEnumerable<PathNode> FindAllPathsFrom(PathNode current) {
            Contract.Requires(current != null);
            Contract.Ensures(Contract.Result<IEnumerable<PathNode>>() != null);

            // enforce this ordering of operations:

            // A) projected to base (geographic or geocentric)
            // B) geographic to geocentric
            // C) geocentric to geographic
            // D) base (geocentric or geographic) to projected

            // steps can be skipped but order must be enforced

            throw new NotImplementedException();
        }


    }
}
