using Pigeoid.CoordinateOperation;
using Pigeoid.Core;
using Pigeoid.Epsg.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{

    public class EpsgCrsCoordinateOperationPathGenerator :
        ICoordinateOperationPathGenerator<EpsgCrs>,
        ICoordinateOperationPathGenerator<ICrs>
    {

        private class EpsgCrsPathSearchNode
        {

            public EpsgCrsPathSearchNode(EpsgCrs crs) {
                Contract.Requires(crs != null);
                Crs = crs;
            }

            public EpsgCrsPathSearchNode(EpsgCrs crs, ICoordinateOperationInfo edgeFromParent, EpsgCrsPathSearchNode parent) {
                Contract.Requires(crs != null);
                Contract.Requires(edgeFromParent != null);
                Contract.Requires(parent != null);
                Crs = crs;
                EdgeFromParent = edgeFromParent;
                Parent = parent;
            }

            public readonly EpsgCrs Crs;
            public readonly ICoordinateOperationInfo EdgeFromParent;
            public readonly EpsgCrsPathSearchNode Parent;
            private bool _opsSet;
            private ushort[] _fromOps;
            private ushort[] _toOps;

            [ContractInvariantMethod]
            private void ObjectInvariants() {
                Contract.Invariant(Crs != null);
            }

            public CoordinateOperationCrsPathInfo BuildCoordinateOperationCrsPathInfo() {
                var nodes = new List<ICrs>();
                var operations = new List<ICoordinateOperationInfo>();

                var current = this;
                do {
                    nodes.Add(current.Crs);
                    if (current.EdgeFromParent == null)
                        break;

                    operations.Add(current.EdgeFromParent);
                    current = current.Parent;
                    Contract.Assume(current != null);
                } while (true/*current != null*/);

                nodes.Reverse();
                operations.Reverse();
                return new CoordinateOperationCrsPathInfo(nodes, operations);
            }

            private void GetOps() {
                if (Crs.Code >= 0 && Crs.Code < UInt16.MaxValue) {
                    var shortCode = unchecked((ushort)Crs.Code);
                    _fromOps = EpsgMicroDatabase.Default.GetOpsFromCrs(shortCode);
                    _toOps = EpsgMicroDatabase.Default.GetOpsToCrs(shortCode);
                }
                _opsSet = true;
            }

            public ushort[] GetFromOps() {
                if (!_opsSet)
                    GetOps();
                return _fromOps;
            }

            public ushort[] GetToOps() {
                if (!_opsSet)
                    GetOps();
                return _toOps;
            }

        }

        private struct OperationNodeCandidate
        {
            public EpsgCoordinateOperationInfoBase CoreOp;
            public double? Accuracy;
            public EpsgCrsPathSearchNode Node;
        }

        private class GeodeticCrsStackItem
        {
            public EpsgCrsGeodetic Crs;
            private bool _opsSet;
            private ushort[] _fromOps;
            private ushort[] _toOps;

            private void GetOps() {
                if (Crs.Code >= 0 && Crs.Code < UInt16.MaxValue) {
                    var shortCode = unchecked((ushort)Crs.Code);
                    _fromOps = EpsgMicroDatabase.Default.GetOpsFromCrs(shortCode);
                    _toOps = EpsgMicroDatabase.Default.GetOpsToCrs(shortCode);
                }
                _opsSet = true;
            }

            public ushort[] GetFromOps() {
                if (!_opsSet)
                    GetOps();
                return _fromOps;
            }

            public ushort[] GetToOps() {
                if (!_opsSet)
                    GetOps();
                return _toOps;
            }
        }

        private class SearchOptions
        {
            public EpsgArea SourceArea;
            public EpsgArea TargetArea;

            public bool IntersectsAll(EpsgArea area) {
                Contract.Requires(area != null);
                if (SourceArea == null) {
                    return TargetArea == null || TargetArea.Intersects(area);
                }
                else {
                    return SourceArea.Intersects(area)
                        && (TargetArea == null || TargetArea.Intersects(area));
                }
            }

        }

        public static EpsgCrs GetEpsgCrs(ICrs crs) {
            Contract.Requires(crs != null);
            var result = crs as EpsgCrs;
            if (result != null)
                return result;

            var authorityTag = crs.Authority;
            if (authorityTag == null)
                return null;

            EpsgAuthorityTag epsgTag;
            if(EpsgAuthorityTag.TryConvert(authorityTag, out epsgTag))
                return EpsgMicroDatabase.Default.GetCrs(epsgTag.NumericalCode);

            return null;
        }


        public List<Predicate<EpsgCrs>> CrsFilters { get; set; } // TODO: make use of this collection
        public List<Predicate<ICoordinateOperationInfo>> OpFilters { get; set; } // TODO: make use of this collection
        private List<GeodeticCrsStackItem> HubCrsList { get; set; }

        public EpsgCrsCoordinateOperationPathGenerator() {
            HubCrsList = new List<GeodeticCrsStackItem>();
            var hubCrs = (EpsgCrsGeodetic)EpsgMicroDatabase.Default.GetCrs(4326);
            while (hubCrs != null) {
                HubCrsList.Add(new GeodeticCrsStackItem { Crs = hubCrs });
                hubCrs = hubCrs.BaseCrs;
            }
        }

        public IEnumerable<EpsgCrs> Hubs {
            get { return HubCrsList.Select(x => x.Crs); }
        }

        public IEnumerable<ICoordinateOperationCrsPathInfo> Generate(ICrs from, ICrs to) {
            if(from == null || to == null)
                return Enumerable.Empty<ICoordinateOperationCrsPathInfo>();

            var fromEpsg = GetEpsgCrs(from);
            var toEpsg = GetEpsgCrs(to);
            if (fromEpsg != null && toEpsg != null)
                return Generate(fromEpsg, toEpsg);

            // TODO: if one of the CRSs is not an EpsgCrs while the other is not, no indirect operations can be used as WGS84 will be required to bridge them
            // TODO: force and indirect path between ICrs->WGS84->EpsgCrs or EpsgCrs->WGS84->ICrs

            return Enumerable.Empty<ICoordinateOperationCrsPathInfo>();
        }

        public IEnumerable<ICoordinateOperationCrsPathInfo> Generate(EpsgCrs from, EpsgCrs to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationCrsPathInfo>>() != null);

            if (from.Kind == EpsgCrsKind.Compound || from.Kind == EpsgCrsKind.Engineering || from.Kind == EpsgCrsKind.Vertical)
                throw new NotImplementedException(String.Format("Support for the from CRS kind {0} is not yet implemented.", from.Kind));
            if (to.Kind == EpsgCrsKind.Compound || to.Kind == EpsgCrsKind.Engineering || to.Kind == EpsgCrsKind.Vertical)
                throw new NotImplementedException(String.Format("Support for the to CRS kind {0} is not yet implemented.", to.Kind));
            if (from.Code == to.Code)
                throw new NotImplementedException("Empty conversion not yet handled.");

            var startNode = new EpsgCrsPathSearchNode(from);

            Contract.Assume(to is EpsgCrsGeodetic);

            var searchRestrictions = new SearchOptions {
                SourceArea = from.Area,
                TargetArea = to.Area
            };

            var corePaths = FindAllCorePaths(startNode, (EpsgCrsGeodetic)to, searchRestrictions);
            return corePaths.Select(node => node.BuildCoordinateOperationCrsPathInfo());
        }

        private EpsgCrsPathSearchNode AppendBacktrackingToStack(EpsgCrsPathSearchNode fromNode, List<GeodeticCrsStackItem> toStack, int startIndex) {
            var node = fromNode;
            for (int backtrackIndex = startIndex; backtrackIndex >= 0; backtrackIndex--) {
                var crs = toStack[backtrackIndex].Crs;
                Contract.Assume(crs.HasBaseOperation);
                var edge = crs.GetBaseOperation();
                node = new EpsgCrsPathSearchNode(crs, edge, node);
            }
            return node;
        }

        private EpsgCrsPathSearchNode FindLowestStackIntersection(List<EpsgCrsPathSearchNode> fromStack, List<GeodeticCrsStackItem> toStack) {
            // first try to locate the lowest CRS where the stacks intersect
            // there is a pretty good chance that most stacks will intersect
            foreach (var fromStackNode in fromStack) {
                var fromStackCrs = fromStackNode.Crs;
                for (int toStackSearchIndex = 0; toStackSearchIndex < toStack.Count; toStackSearchIndex++) {
                    var toStackCrs = toStack[toStackSearchIndex];

                    // it does not make much sense to do a full search once an intersection is found as the lowest will be best
                    if (fromStackCrs.Code == toStackCrs.Crs.Code)
                        return AppendBacktrackingToStack(fromStackNode, toStack, toStackSearchIndex - 1);
                }
            }
            return null;
        }

        private IEnumerable<OperationNodeCandidate> CreateForwardNodeCandidates(IEnumerable<ushort> forwardOperationCodes, EpsgCrs targetCrs, EpsgCrsPathSearchNode parentNode) {
            Contract.Requires(forwardOperationCodes != null);
            Contract.Ensures(Contract.Result<IEnumerable<OperationNodeCandidate>>() != null);
            return forwardOperationCodes
                .Select(code => EpsgMicroDatabase.Default.GetCoordinateTransformOrConcatenatedInfo(code))
                .Where(op => op != null)
                .Select(op => new OperationNodeCandidate {
                    CoreOp = op,
                    Accuracy = op.Accuracy,
                    Node = new EpsgCrsPathSearchNode(targetCrs, op, parentNode)
                });
        }

        private IEnumerable<OperationNodeCandidate> CreateInverseNodeCandidates(IEnumerable<ushort> inverseOperationCodes, EpsgCrs targetCrs, EpsgCrsPathSearchNode parentNode) {
            Contract.Requires(inverseOperationCodes != null);
            Contract.Ensures(Contract.Result<IEnumerable<OperationNodeCandidate>>() != null);
            return inverseOperationCodes
                .Select(code => EpsgMicroDatabase.Default.GetCoordinateTransformOrConcatenatedInfo(code))
                .Where(op => op != null && op.HasInverse)
                .Select(op => new OperationNodeCandidate {
                    CoreOp = op,
                    Accuracy = op.Accuracy,
                    Node = new EpsgCrsPathSearchNode(targetCrs, op.GetInverse(), parentNode)
                });
        }

        private IEnumerable<OperationNodeCandidate> CreateNodeCandidates(IEnumerable<ushort> forwardOperationCodes, IEnumerable<ushort> inverseOperationCodes, EpsgCrs targetCrs, EpsgCrsPathSearchNode parentNode) {
            IEnumerable<OperationNodeCandidate> nodeCandidates = null;
            if (forwardOperationCodes != null) {
                Contract.Assume(nodeCandidates == null);
                nodeCandidates = CreateForwardNodeCandidates(forwardOperationCodes, targetCrs, parentNode);
            }
            if (inverseOperationCodes != null) {
                var inverseOpNodes = CreateInverseNodeCandidates(inverseOperationCodes, targetCrs, parentNode);
                nodeCandidates = nodeCandidates == null ? inverseOpNodes : nodeCandidates.Concat(inverseOpNodes);
            }
            if(nodeCandidates == null)
                return Enumerable.Empty<OperationNodeCandidate>();

            return nodeCandidates
                .OrderBy(x => x.CoreOp.Deprecated)
                .ThenByDescending(x => x.Accuracy.HasValue)
                .ThenBy(x => x.Accuracy.GetValueOrDefault());
        }

        private IEnumerable<EpsgCrsPathSearchNode> FindDirectTransformations(List<EpsgCrsPathSearchNode> fromStack, List<GeodeticCrsStackItem> toStack, SearchOptions searchOptions) {
            // TODO: make sure that operations respect the area of from & to
            var results = new List<EpsgCrsPathSearchNode>(); // TODO: Consider changing this to yield return
            foreach (var fromNode in fromStack) {
                var fromOpCodesForward = fromNode.GetFromOps();
                var fromOpCodesInverse = fromNode.GetToOps();
                if (fromOpCodesForward == null && fromOpCodesInverse == null)
                    continue;

                for (int toCrsIndex = 0; toCrsIndex < toStack.Count; toCrsIndex++) {
                    var toData = toStack[toCrsIndex];
                    var toOpCodesForward = toData.GetToOps();
                    var toOpCodesInverse = toData.GetFromOps();
                    if (toOpCodesForward == null && toOpCodesInverse == null)
                        continue;

                    var forwardIntersection = fromOpCodesForward == null || toOpCodesForward == null ? null : fromOpCodesForward.Intersect(toOpCodesForward);
                    var inverseIntersection = fromOpCodesInverse == null || toOpCodesInverse == null ? null : fromOpCodesInverse.Intersect(toOpCodesInverse);

                    foreach (var candidate in CreateNodeCandidates(forwardIntersection, inverseIntersection, toData.Crs, fromNode)) {
                        var candidateOp = candidate.CoreOp;
                        if (candidateOp.Area != null && !searchOptions.IntersectsAll(candidateOp.Area))
                            continue;

                        var fullPath = AppendBacktrackingToStack(candidate.Node, toStack, toCrsIndex - 1);
                        results.Add(fullPath);
                    }
                }

            }
            return results;
        }

        private IEnumerable<EpsgCrsPathSearchNode> FindIndirectTransformations(List<EpsgCrsPathSearchNode> fromStack, List<GeodeticCrsStackItem> toStack, SearchOptions searchOptions) {
            // TODO: make sure that operations respect the area of from & to
            var results = new List<EpsgCrsPathSearchNode>(); // TODO: Consider changing this to yield return
            foreach (var fromNode in fromStack) {
                var fromOpCodesForward = fromNode.GetFromOps();
                var fromOpCodesInverse = fromNode.GetToOps();
                if (fromOpCodesForward == null && fromOpCodesInverse == null)
                    continue;

                foreach (var hubNode in HubCrsList) {
                    var toHubForward = hubNode.GetToOps();
                    var toHubInverse = hubNode.GetFromOps();
                    if (toHubForward == null && toHubInverse == null)
                        continue; // no path to the hub

                    var forwardToHubIntersection = fromOpCodesForward == null || toHubForward == null ? null : fromOpCodesForward.Intersect(toHubForward);
                    var inverseToHubIntersection = fromOpCodesInverse == null || toHubInverse == null ? null : fromOpCodesInverse.Intersect(toHubInverse);

                    foreach (var toHubNode in CreateNodeCandidates(forwardToHubIntersection, inverseToHubIntersection, hubNode.Crs, fromNode)) {
                        var toHubCandidateOp = toHubNode.CoreOp;
                        if (toHubCandidateOp.Area != null && !searchOptions.IntersectsAll(toHubCandidateOp.Area))
                            continue;

                        var fromHubForward = toHubInverse;
                        var fromHubInverse = toHubForward;

                        for (int toCrsIndex = 0; toCrsIndex < toStack.Count; toCrsIndex++) {
                            var toData = toStack[toCrsIndex];
                            var toOpCodesForward = toData.GetToOps();
                            var toOpCodesInverse = toData.GetFromOps();
                            if (toOpCodesForward == null && toOpCodesInverse == null)
                                continue; // no path to the destination

                            var forwardFromHubIntersection = fromHubForward == null || toOpCodesForward == null ? null : fromHubForward.Intersect(toOpCodesForward);
                            var inverseFromHubIntersection = fromHubInverse == null || toOpCodesInverse == null ? null : fromHubInverse.Intersect(toOpCodesInverse);

                            foreach (var candidate in CreateNodeCandidates(forwardFromHubIntersection, inverseFromHubIntersection, toData.Crs, toHubNode.Node)) {
                                var fromHubCandidateOp = candidate.CoreOp;
                                if (fromHubCandidateOp.Area != null && !searchOptions.IntersectsAll(fromHubCandidateOp.Area))
                                    continue;

                                var fullPath = AppendBacktrackingToStack(candidate.Node, toStack, toCrsIndex - 1);
                                results.Add(fullPath);
                            }

                        }

                    }

                }

            }
            return results;
        }

        private IEnumerable<EpsgCrsPathSearchNode> FindAllCorePaths(EpsgCrsPathSearchNode fromNode, EpsgCrsGeodetic toCrs, SearchOptions searchOptions) {
            Contract.Requires(fromNode != null);
            Contract.Requires(fromNode.Crs is EpsgCrsGeodetic);
            Contract.Requires(toCrs != null);

            var earlyResults = new List<EpsgCrsPathSearchNode>();
            var fromCrs = (EpsgCrsGeodetic)fromNode.Crs;

            // construct the hierarchy based on the from CRS
            var fromStack = new List<EpsgCrsPathSearchNode>();
            var fromStackConstructionNode = fromNode;
            do {
                fromStack.Add(fromStackConstructionNode);
                var currentCrs = (EpsgCrsGeodetic)fromStackConstructionNode.Crs;
                if (!currentCrs.HasBaseOperation)
                    break;

                var baseCrs = currentCrs.BaseCrs;
                var fromBaseEdge = currentCrs.GetBaseOperation();
                Contract.Assume(baseCrs != null);
                Contract.Assume(fromBaseEdge != null);
                
                if (!fromBaseEdge.HasInverse)
                    break; // we have to invert the edge to traverse up the stack

                var toBaseEdge = fromBaseEdge.GetInverse();
                fromStackConstructionNode = new EpsgCrsPathSearchNode(baseCrs, toBaseEdge, fromStackConstructionNode);
            } while (true/*fromStackSearchNode != null*/);

            // construct the hierarchy based on the to CRS
            var toStack = new List<GeodeticCrsStackItem>();
            var toStackConstructionCrs = toCrs;
            do {
                toStack.Add(new GeodeticCrsStackItem { Crs = toStackConstructionCrs });
                toStackConstructionCrs = toStackConstructionCrs.BaseCrs;
            } while (toStackConstructionCrs != null);

            var lowestStackIntersection = FindLowestStackIntersection(fromStack, toStack);
            if (lowestStackIntersection != null)
                earlyResults.Add(lowestStackIntersection);

            var directResults = FindDirectTransformations(fromStack, toStack, searchOptions);
            var indirectResults = FindIndirectTransformations(fromStack, toStack, searchOptions);

            return earlyResults.Concat(directResults).Concat(indirectResults);
        }

    }
}
