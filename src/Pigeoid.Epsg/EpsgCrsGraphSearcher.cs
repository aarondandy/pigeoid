using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg.Utility;
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
                Crs = crs;
            }

            [ContractInvariantMethod]
            private void ObjectInvariants() {
                Contract.Invariant(Crs != null);
            }

            public PathNode Parent;
            public ICoordinateOperationInfo EdgeFromParent;
            public readonly EpsgCrs Crs;

            public int PathLength {
                get {
                    var c = 0;
                    var pathNode = this;
                    do {
                        c++;
                        pathNode = pathNode.Parent;
                    } while (pathNode != null);
                    return c;
                }
            }

            public IEnumerable<int> GetCrsCodes() {
                var pathNode = this;
                do {
                    yield return pathNode.Crs.Code;
                    pathNode = pathNode.Parent;
                } while (pathNode != null);
            }

            public bool HasNode(int crsCode) {
                var pathNode = this;
                do {
                    var crs = pathNode.Crs;
                    if (crs.Code == crsCode)
                        return true;

                    pathNode = pathNode.Parent;
                } while (pathNode != null);
                return false;
            }

            public bool HasUsedEdge(int edgeCode) {
                var pathNode = this;
                do {
                    var edge = pathNode.EdgeFromParent;
                    if (edge == null) {
                        Contract.Assume(pathNode.Parent == null);
                        return false;
                    }

                    var coordOpBase = edge as EpsgCoordinateOperationInfoBase;
                    if (coordOpBase != null) {
                        if (coordOpBase.Code == edgeCode)
                            return true;
                    }
                    else {
                        var coordOpInverse = edge as EpsgCoordinateOperationInverse;
                        if (coordOpInverse != null) {
                            if (coordOpInverse.Core.Code == edgeCode)
                                return true;
                        }
                    }

                    pathNode = pathNode.Parent;
                    Contract.Assume(pathNode != null); // because pathNode.EdgeFromParent was checked
                } while (true); // pathNode != null
            }

            public PathNode Append(EpsgCrs crs, ICoordinateOperationInfo edgeFromParent) {
                var newNode = new PathNode(crs) {
                    Parent = this,
                    EdgeFromParent = edgeFromParent
                };
                return newNode;
            }

            public CoordinateOperationCrsPathInfo CreateCoordinateOperationCrsPathInfo() {
                var nodes = new List<ICrs>();
                var operations = new List<ICoordinateOperationInfo>();

                var current = this;
                do {

                    nodes.Add(current.Crs);
                    if (current.EdgeFromParent != null) {
                        operations.Add(current.EdgeFromParent);
                        current = current.Parent;
                        Contract.Assume(current != null);
                    }
                    else {
                        break;
                    }

                } while (true/*current != null*/);

                nodes.Reverse();
                operations.Reverse();

                return new CoordinateOperationCrsPathInfo(nodes, operations);
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

            foreach (var pathNode in allPaths) {
                yield return pathNode.CreateCoordinateOperationCrsPathInfo();
            }
        }

        private IEnumerable<PathNode> FindAllPathsFrom(PathNode current) {
            Contract.Requires(current != null);
            Contract.Ensures(Contract.Result<IEnumerable<PathNode>>() != null);

            var results = new List<PathNode>();
            var currentCode = current.Crs.Code;

            var visitedCrsCodes = new HashSet<int>(current.GetCrsCodes());

            if (currentCode == TargetCrs.Code)
                return ArrayUtil.CreateSingleElementArray(current);

            // projections with this as a base
            var derrivedProjectionCodes = EpsgCrsProjected.GetProjectionCodesBasedOn(currentCode);
            if (derrivedProjectionCodes.Count != 0) {

                foreach (var derrivedProjectionCode in derrivedProjectionCodes) {
                    if (visitedCrsCodes.Contains(derrivedProjectionCode))
                        continue;

                    var derrivedProjection = EpsgCrsProjected.GetProjected(derrivedProjectionCode);
                    var projection = derrivedProjection.Projection;
                    var nextNode = current.Append(derrivedProjection, projection);

                    // TODO: make sure this path is OK

                    var localResults = FindAllPathsFrom(nextNode);
                    results.AddRange(localResults);
                }
            }

            if (current.Crs.Kind == EpsgCrsKind.Projected) {
                var projectedCrs = (EpsgCrsProjected)current.Crs;
                var baseCrs = projectedCrs.BaseCrs;
                if (!visitedCrsCodes.Contains(baseCrs.Code)) {
                    var projection = projectedCrs.Projection;
                    if (baseCrs != null && projection != null && projection.HasInverse) {
                        // TODO: make sure this path is OK
                        var nextNode = current.Append(baseCrs, projection.GetInverse());
                        var localResults = FindAllPathsFrom(nextNode);
                        results.AddRange(localResults);
                    }
                }
            }

            var concatenatedForward = EpsgCoordinateOperationInfoRepository.GetConcatenatedForwardReferenced(currentCode);
            foreach (var catOp in concatenatedForward) {
                if (visitedCrsCodes.Contains(catOp.TargetCrsCode))
                    continue;

                var targetCrs = catOp.TargetCrs;
                // TODO: make sure this path is OK
                var nextNode = current.Append(targetCrs, catOp);
                var localResults = FindAllPathsFrom(nextNode);
                results.AddRange(localResults);
            }
            var concatenatedReverse = EpsgCoordinateOperationInfoRepository.GetConcatenatedReverseReferenced(currentCode);
            foreach (var catOp in concatenatedReverse) {
                if (!catOp.HasInverse)
                    continue;
                if (visitedCrsCodes.Contains(catOp.SourceCrsCode))
                    continue;

                var sourceCrs = catOp.SourceCrs;
                // TODO: make sure this path is OK
                var nextNode = current.Append(sourceCrs, catOp.GetInverse());
                var localResults = FindAllPathsFrom(nextNode);
                results.AddRange(localResults);
            }
            var transformForward = EpsgCoordinateOperationInfoRepository.GetTransformForwardReferenced(currentCode);
            foreach (var txOp in transformForward) {
                if (visitedCrsCodes.Contains(txOp.TargetCrsCode))
                    continue;

                var targetCrs = txOp.TargetCrs;
                // TODO: make sure this path is OK
                var nextNode = current.Append(targetCrs, txOp);
                var localResults = FindAllPathsFrom(nextNode);
                results.AddRange(localResults);
            }
            var transformReverse = EpsgCoordinateOperationInfoRepository.GetTransformReverseReferenced(currentCode);
            foreach (var txOp in transformReverse) {
                if (!txOp.HasInverse)
                    continue;
                if (visitedCrsCodes.Contains(txOp.SourceCrsCode))
                    continue;

                var sourceCrs = txOp.SourceCrs;
                // TODO: make sure this path is OK
                var nextNode = current.Append(sourceCrs, txOp.GetInverse());
                var localResults = FindAllPathsFrom(nextNode);
                results.AddRange(localResults);
            }

            return results;
        }


    }
}
