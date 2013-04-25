using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid
{
    public class CoordinateOperationCrsPathInfo : ICoordinateOperationCrsPathInfo
    {

        private static readonly ReadOnlyCollection<ICoordinateOperationInfo> EmptyEdgeList = Array.AsReadOnly(new ICoordinateOperationInfo[0]);

        public static CoordinateOperationCrsPathInfo Join(CoordinateOperationCrsPathInfo primary, CoordinateOperationCrsPathInfo other) {
            if (primary == null) throw new ArgumentNullException("primary");
            if (other == null) throw new ArgumentNullException("other");
            Contract.Ensures(Contract.Result<CoordinateOperationCrsPathInfo>() != null);
            // TODO: make sure that the last node of the first list and the first node of the last list are a match
            var nodes = new ICrs[primary._crsNodes.Count + other._crsNodes.Count - 1];
            primary._crsNodes.CopyTo(nodes, 0);
            other._crsNodes.CopyTo(nodes, primary._crsNodes.Count - 1);
            var edges = new ICoordinateOperationInfo[primary._opEdges.Count + other._opEdges.Count];
            primary._opEdges.CopyTo(edges, 0);
            other._opEdges.CopyTo(edges, primary._opEdges.Count);
            return new CoordinateOperationCrsPathInfo(nodes, edges);
        }

        public CoordinateOperationCrsPathInfo(ICrs node) {
            _crsNodes = Array.AsReadOnly(new[] { node });
            _opEdges = EmptyEdgeList;
        }

        public CoordinateOperationCrsPathInfo(IEnumerable<ICrs> nodes, IEnumerable<ICoordinateOperationInfo> edges)
            : this(nodes.ToArray(), edges.ToArray()) { }

        private CoordinateOperationCrsPathInfo(ICrs[] nodes, ICoordinateOperationInfo[] edges) {
            _crsNodes = Array.AsReadOnly(nodes);
            _opEdges = Array.AsReadOnly(edges);
            if ((_opEdges.Count + 1) != _crsNodes.Count)
                throw new ArgumentException("There must be exactly one more node than edge.");
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_crsNodes != null);
            Contract.Invariant(_opEdges != null);
        }

        private readonly ReadOnlyCollection<ICrs> _crsNodes;
        private readonly ReadOnlyCollection<ICoordinateOperationInfo> _opEdges;

        public IEnumerable<ICrs> CoordinateReferenceSystems {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICrs>>() != null);
                return _crsNodes;
            }
        }

        public IEnumerable<ICoordinateOperationInfo> CoordinateOperations {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>() != null);
                return _opEdges;
            }
        }

        [Pure]
        public CoordinateOperationCrsPathInfo Append(ICrs node, ICoordinateOperationInfo edge) {
            if(node == null) throw new ArgumentNullException("node");
            if(edge == null) throw new ArgumentNullException("edge");
            Contract.Ensures(Contract.Result<CoordinateOperationCrsPathInfo>() != null);
            var nodes = new ICrs[_crsNodes.Count + 1];
            _crsNodes.CopyTo(nodes, 0);
            nodes[nodes.Length - 1] = node;
            var edges = new ICoordinateOperationInfo[_opEdges.Count + 1];
            _opEdges.CopyTo(edges, 0);
            edges[edges.Length - 1] = edge;
            return new CoordinateOperationCrsPathInfo(nodes, edges);
        }

        [Pure]
        public CoordinateOperationCrsPathInfo Prepend(ICrs node, ICoordinateOperationInfo edge) {
            if (node == null) throw new ArgumentNullException("node");
            if (edge == null) throw new ArgumentNullException("edge");
            Contract.Ensures(Contract.Result<CoordinateOperationCrsPathInfo>() != null);
            var nodes = new ICrs[_crsNodes.Count + 1];
            _crsNodes.CopyTo(nodes, 1);
            nodes[0] = node;
            var edges = new ICoordinateOperationInfo[_opEdges.Count + 1];
            _opEdges.CopyTo(edges, 1);
            edges[0] = edge;
            return new CoordinateOperationCrsPathInfo(nodes, edges);
        }

        [Pure]
        public CoordinateOperationCrsPathInfo Append(CoordinateOperationCrsPathInfo other) {
            Contract.Requires(other != null);
            Contract.Ensures(Contract.Result<CoordinateOperationCrsPathInfo>() != null);
            return Join(this, other);
        }
    }
}
