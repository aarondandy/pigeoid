using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{

    internal class EpsgCrsPathSearchNode
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

        [Obsolete("Remove if not used.")]
        private static int GetEdgeCode(ICoordinateOperationInfo op) {
            var epsgOp = op as EpsgCoordinateOperationInfoBase;
            if(epsgOp == null && op is EpsgCoordinateOperationInverse)
                epsgOp = ((EpsgCoordinateOperationInverse)op).GetInverse();

            return epsgOp != null ? epsgOp.Code : 0;
        }

        [Obsolete("Remove if not used.")]
        public bool ContainsNode(EpsgCrsPathSearchNode node) {
            Contract.Requires(node != null);
            return ContainsNode(node.Crs.Code, GetEdgeCode(node.EdgeFromParent));
        }

        [Obsolete("Remove if not used.")]
        public bool ContainsNode(int crsCode, int edgeCode) {
            var searchTarget = this;
            do {
                if (searchTarget.Crs.Code == crsCode) {
                    var searchEdgeCode = GetEdgeCode(searchTarget.EdgeFromParent);
                    if (searchEdgeCode == edgeCode)
                        return true;
                }
                searchTarget = searchTarget.Parent;
            } while (searchTarget != null);
            return false;
        }

    }

    public class EpsgCrsCoordinateOperationPathGenerator :
        ICoordinateOperationPathGenerator<EpsgCrs>,
        ICoordinateOperationPathGenerator<ICrs>
    {

        public List<Predicate<EpsgCrs>> CrsFilters { get; set; }
        public List<Predicate<ICoordinateOperationInfo>> OpFilters { get; set; }

        public IEnumerable<ICoordinateOperationCrsPathInfo> Generate(ICrs from, ICrs to) {
            var fromEpsg = from as EpsgCrs;
            var toEpsg = to as EpsgCrs;
            if (fromEpsg != null && toEpsg != null)
                return Generate(fromEpsg, toEpsg);
            throw new NotImplementedException();
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
            var corePaths = FindAllCorePaths(startNode, (EpsgCrsGeodetic)to);
            return corePaths.Select(node => node.BuildCoordinateOperationCrsPathInfo());
        }

        private EpsgCrsPathSearchNode AppendBacktrackingToStack(EpsgCrsPathSearchNode fromNode, List<EpsgCrsGeodetic> toStack, int startIndex) {
            var node = fromNode;
            for (int backtrackIndex = startIndex; backtrackIndex >= 0; backtrackIndex--) {
                var crs = toStack[backtrackIndex];
                Contract.Assume(crs.HasBaseOperation);
                var edge = crs.GetBaseOperation();
                node = new EpsgCrsPathSearchNode(crs, edge, node);
            }
            return node;
        }

        private EpsgCrsPathSearchNode FindLowestStackIntersection(List<EpsgCrsPathSearchNode> fromStack, List<EpsgCrsGeodetic> toStack) {
            // first try to locate the lowest CRS where the stacks intersect
            // there is a pretty good chance that most stacks will intersect
            // it does not make much sense to do a full search once an intersection is found as the lowest will be best
            foreach (var fromStackNode in fromStack) {
                var fromStackCrs = fromStackNode.Crs;
                for (int toStackSearchIndex = 0; toStackSearchIndex < toStack.Count; toStackSearchIndex++) {
                    var toStackCrs = toStack[toStackSearchIndex];
                    if (fromStackCrs.Code == toStackCrs.Code)
                        return AppendBacktrackingToStack(fromStackNode, toStack, toStackSearchIndex - 1);
                }
            }
            return null;
        }

        private IEnumerable<EpsgCrsPathSearchNode> FindDirectTransformations(List<EpsgCrsPathSearchNode> fromStack, List<EpsgCrsGeodetic> toStack) {
            var results = new List<EpsgCrsPathSearchNode>();
            foreach (var fromNode in fromStack) {
                var fromCrs = fromNode.Crs;
                if (fromCrs.Code < 0 || fromCrs.Code > UInt16.MaxValue)
                    continue;

                var fromCrsCodeShort = (ushort)fromCrs.Code;
                var fromOpCodesForward = EpsgMicroDatabase.Default.GetOpsFromCrs(fromCrsCodeShort);
                var fromOpCodesInverse = EpsgMicroDatabase.Default.GetOpsToCrs(fromCrsCodeShort);

                if (fromOpCodesForward == null && fromOpCodesInverse == null)
                    continue;

                for (int toCrsIndex = 0; toCrsIndex < toStack.Count; toCrsIndex++) {
                    var toCrs = toStack[toCrsIndex];
                    if (toCrs.Code < 0 || toCrs.Code > UInt16.MaxValue)
                        continue;

                    var toCrsCodeShort = (ushort)toCrs.Code;
                    var toOpCodesForward = EpsgMicroDatabase.Default.GetOpsToCrs(toCrsCodeShort);
                    var toOpCodesInverse = EpsgMicroDatabase.Default.GetOpsFromCrs(toCrsCodeShort);

                    var forwardIntersection = fromOpCodesForward == null || toOpCodesForward == null ? null : fromOpCodesForward.Intersect(toOpCodesForward).ToList();
                    var inverseIntersection = fromOpCodesInverse == null || toOpCodesInverse == null ? null : fromOpCodesInverse.Intersect(toOpCodesInverse).ToList();
                    
                    if (forwardIntersection != null) {
                        var toJoinNodes = forwardIntersection
                            .Select(code => EpsgMicroDatabase.Default.GetCoordinateTransformInfo(code))
                            .Where(op => op != null)
                            .Select(op => new EpsgCrsPathSearchNode(toCrs, op, fromNode))
                            .ToList();
                        foreach (var n in toJoinNodes) {
                            var fullPath = AppendBacktrackingToStack(n, toStack, toCrsIndex-1);
                            results.Add(fullPath);
                        }
                    }
                    if (inverseIntersection != null) {
                        var toJoinNodes = inverseIntersection
                            .Select(code => EpsgMicroDatabase.Default.GetCoordinateTransformInfo(code))
                            .Where(op => op != null && op.HasInverse)
                            .Select(op => new EpsgCrsPathSearchNode(toCrs, op.GetInverse(), fromNode))
                            .ToList();
                        foreach (var n in toJoinNodes) {
                            var fullPath = AppendBacktrackingToStack(n, toStack, toCrsIndex - 1);
                            results.Add(fullPath);
                        }
                    }
                }

            }
            return results;
        }

        private IEnumerable<EpsgCrsPathSearchNode> FindIndirectTransformations(List<EpsgCrsPathSearchNode> fromStack, List<EpsgCrsGeodetic> toStack) {
            var results = new List<EpsgCrsPathSearchNode>();
            return results;
        }

        private IEnumerable<EpsgCrsPathSearchNode> FindAllCorePaths(EpsgCrsPathSearchNode fromNode, EpsgCrsGeodetic toCrs) {
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
            var toStack = new List<EpsgCrsGeodetic>();
            var toStackConstructionCrs = toCrs;
            do {
                toStack.Add(toStackConstructionCrs);
                toStackConstructionCrs = toStackConstructionCrs.BaseCrs;
            } while (toStackConstructionCrs != null);

            var lowestStackIntersection = FindLowestStackIntersection(fromStack, toStack);
            if (lowestStackIntersection != null)
                earlyResults.Add(lowestStackIntersection);

            var directResults = FindDirectTransformations(fromStack, toStack);
            var indirectResults = FindIndirectTransformations(fromStack, toStack);

            return earlyResults.Concat(directResults).Concat(indirectResults);
        }

    }
}
