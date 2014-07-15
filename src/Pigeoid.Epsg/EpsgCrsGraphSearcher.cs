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

            public EpsgCrsKind CrsKind {
                get {
                    return Crs.Kind;
                }
            }

            public EpsgCrsKind ParentKind {
                get {
                    if (Parent == null)
                        return EpsgCrsKind.Unknown;
                    return Parent.CrsKind;
                }
            }

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
                Contract.Requires(edgeFromParent != null);
                Contract.Ensures(Contract.Result<PathNode>() != null);
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
            SourceArea = sourceCrs.Area;
            TargetCrs = targetCrs;
            TargetCrsCode = targetCrs.Code;
            TargetArea = targetCrs.Area;
        }

        public EpsgCrs SourceCrs { get; private set; }

        public EpsgArea SourceArea { get; protected set; }

        private readonly int SourceCrsCode;

        private readonly int TargetCrsCode;

        public EpsgCrs TargetCrs { get; private set; }

        public EpsgArea TargetArea { get; protected set; }

        public List<Predicate<EpsgCrs>> CrsFilters { get; set; }
        public List<Predicate<ICoordinateOperationInfo>> OpFilters { get; set; }

        public bool AreaIntersectionTest(EpsgCrs crs) {
            Contract.Requires(crs != null);
            var area = crs.Area;
            return area == null
                || SourceArea == null
                || SourceArea.Intersects(area)
                || TargetArea == null
                || TargetArea.Intersects(area);
        }

        public bool AreaContainsTest(EpsgCrs crs) {
            Contract.Requires(crs != null);
            var area = crs.Area;
            return area == null
                || SourceArea == null
                || area.Contains(SourceArea)
                || TargetArea == null
                || area.Contains(TargetArea);
        }

        private bool IsCrsAllowed(EpsgCrs crs) {
            Contract.Requires(crs != null);
            if (CrsFilters == null || CrsFilters.Count == 0)
                return true;

            return CrsFilters.All(f => f(crs));
        }

        private bool IsOpAllowed(ICoordinateOperationInfo op) {
            Contract.Requires(op != null);
            if (OpFilters == null || OpFilters.Count == 0)
                return true;

            return OpFilters.All(f => f(op));
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(SourceCrs != null);
            Contract.Invariant(TargetCrs != null);
        }

        public IEnumerable<ICoordinateOperationCrsPathInfo> FindAllPaths() {
            var rootNode = PathNode.CreateStartNode(SourceCrs);
            var allPaths = FindAllPathsFrom(rootNode, false, false).ToList(); // TODO: optimize
            return allPaths.Select(p => p.CreateCoordinateOperationCrsPathInfo());
        }

        private IEnumerable<PathNode> FindAllPathsFrom(PathNode current, bool mustMoveToProjected, bool transformationRestricted) {
            Contract.Requires(current != null);
            Contract.Ensures(Contract.Result<IEnumerable<PathNode>>() != null);

            var currentCode = current.Crs.Code;
            if (currentCode == TargetCrs.Code)
                return ArrayUtil.CreateSingleElementArray(current);

            var currentKind = current.CrsKind;
            var visitedCrsCodes = new HashSet<int>(current.GetCrsCodes());
            var results = new List<PathNode>();

            var derrivedProjectionCodes = EpsgCrsProjected.GetProjectionCodesBasedOn(currentCode);
            if (derrivedProjectionCodes.Count != 0) {
                // move down to projected from here, all new nodes must now move towards projected
                foreach (var derrivedProjectionCode in derrivedProjectionCodes) {
                    if (visitedCrsCodes.Contains(derrivedProjectionCode))
                        continue;

                    var derrivedProjection = EpsgCrsProjected.GetProjected(derrivedProjectionCode);
                    if (!IsCrsAllowed(derrivedProjection))
                        continue;

                    var projection = derrivedProjection.Projection;
                    if (!IsOpAllowed(projection))
                        continue;

                    var nextNode = current.Append(derrivedProjection, projection);
                    var localResults = FindAllPathsFrom(nextNode, true, transformationRestricted);
                    results.AddRange(localResults);
                }
            }

            if (currentKind == EpsgCrsKind.Projected && !mustMoveToProjected) {
                // can only move away from projected if not restricted and if this is projected
                var projectedCrs = (EpsgCrsProjected)current.Crs;
                var baseCrs = projectedCrs.BaseCrs;
                if (baseCrs != null && !visitedCrsCodes.Contains(baseCrs.Code) && IsCrsAllowed(baseCrs)) {
                    var projection = projectedCrs.Projection;
                    if (projection != null && projection.HasInverse) {
                        var projectionInverse = projection.GetInverse();
                        if (IsOpAllowed(projectionInverse)) {
                            var nextNode = current.Append(baseCrs, projectionInverse);
                            var localResults = FindAllPathsFrom(nextNode, false, transformationRestricted);
                            results.AddRange(localResults);
                        }
                    }
                }
            }

            if (!transformationRestricted) {

                var concatenatedForward = EpsgCoordinateOperationInfoRepository.GetConcatenatedForwardReferenced(currentCode);
                foreach (var catOp in concatenatedForward) {
                    if (visitedCrsCodes.Contains(catOp.TargetCrsCode))
                        continue;
                    if (!IsOpAllowed(catOp))
                        continue;

                    // TODO: is this path OK?
                    // NOTE: ignore mustMoveToProjected ... for now
                    var targetCrs = catOp.TargetCrs;
                    if (!IsCrsAllowed(targetCrs))
                        continue;
                    var nextNode = current.Append(targetCrs, catOp);
                    var localResults = FindAllPathsFrom(nextNode, mustMoveToProjected, true);
                    results.AddRange(localResults);
                }

                var concatenatedReverse = EpsgCoordinateOperationInfoRepository.GetConcatenatedReverseReferenced(currentCode);
                foreach (var catOp in concatenatedReverse) {
                    if (!catOp.HasInverse)
                        continue;
                    if (visitedCrsCodes.Contains(catOp.SourceCrsCode))
                        continue;
                    var inverseCatOp = catOp.GetInverse();
                    if (!IsOpAllowed(inverseCatOp))
                        continue;

                    // TODO: is this path OK?
                    // NOTE: ignore mustMoveToProjected ... for now
                    var sourceCrs = catOp.SourceCrs;
                    if (!IsCrsAllowed(sourceCrs))
                        continue;
                    var nextNode = current.Append(sourceCrs, inverseCatOp);
                    var localResults = FindAllPathsFrom(nextNode, mustMoveToProjected, true);
                    results.AddRange(localResults);
                }

                var transformForward = EpsgCoordinateOperationInfoRepository.GetTransformForwardReferenced(currentCode);
                foreach (var txOp in transformForward) {
                    if (visitedCrsCodes.Contains(txOp.TargetCrsCode))
                        continue;
                    if (!IsOpAllowed(txOp))
                        continue;

                    // TODO: is this path OK?
                    // TODO: enforce mustMoveToProjected
                    var targetCrs = txOp.TargetCrs;
                    if (!IsCrsAllowed(targetCrs))
                        continue;
                    var nextNode = current.Append(targetCrs, txOp);
                    var localResults = FindAllPathsFrom(nextNode, mustMoveToProjected, true);
                    results.AddRange(localResults);
                }
                var transformReverse = EpsgCoordinateOperationInfoRepository.GetTransformReverseReferenced(currentCode);
                foreach (var txOp in transformReverse) {
                    if (!txOp.HasInverse)
                        continue;
                    if (visitedCrsCodes.Contains(txOp.SourceCrsCode))
                        continue;
                    var inverseOp = txOp.GetInverse();
                    if (!IsOpAllowed(inverseOp))
                        continue;

                    // TODO: is this path OK?
                    // TODO: enforce mustMoveToProjected
                    var sourceCrs = txOp.SourceCrs;
                    if (!IsCrsAllowed(sourceCrs))
                        continue;
                    var nextNode = current.Append(sourceCrs, inverseOp);
                    var localResults = FindAllPathsFrom(nextNode, mustMoveToProjected, true);
                    results.AddRange(localResults);
                }
            }

            return results;
        }

    }
}
