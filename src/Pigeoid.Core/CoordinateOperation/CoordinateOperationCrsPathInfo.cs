using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Pigeoid.CoordinateOperation
{
    public class CoordinateOperationCrsPathInfo : ICoordinateOperationCrsPathInfo
    {

        public static CoordinateOperationCrsPathInfo Join(CoordinateOperationCrsPathInfo primary, CoordinateOperationCrsPathInfo other) {
            if (primary == null) throw new ArgumentNullException("primary");
            if (other == null) throw new ArgumentNullException("other");
            Contract.Ensures(Contract.Result<CoordinateOperationCrsPathInfo>() != null);
            // TODO: make sure that the last node of the first list and the first node of the last list are a match
            var nodes = new ICrs[primary.CrsNodesArray.Length + other.CrsNodesArray.Length - 1];
            primary.CrsNodesArray.CopyTo(nodes, 0);
            other.CrsNodesArray.CopyTo(nodes, primary.CrsNodesArray.Length - 1);
            var edges = new ICoordinateOperationInfo[primary.OperationEdgesArray.Length + other.OperationEdgesArray.Length];
            primary.OperationEdgesArray.CopyTo(edges, 0);
            other.OperationEdgesArray.CopyTo(edges, primary.OperationEdgesArray.Length);
            return new CoordinateOperationCrsPathInfo(nodes, edges);
        }

        public CoordinateOperationCrsPathInfo(ICrs node) {
            CrsNodesArray = new[] { node };
            OperationEdgesArray = new ICoordinateOperationInfo[0];
        }

        public CoordinateOperationCrsPathInfo(IEnumerable<ICrs> nodes, IEnumerable<ICoordinateOperationInfo> edges)
            : this(nodes.ToArray(), edges.ToArray()) {
            Contract.Requires(nodes != null);
            Contract.Requires(edges != null);
        }

        private CoordinateOperationCrsPathInfo(ICrs[] nodes, ICoordinateOperationInfo[] edges) {
            Contract.Requires(nodes != null);
            Contract.Requires(edges != null);
            CrsNodesArray = nodes;
            OperationEdgesArray = edges;
            if ((OperationEdgesArray.Length + 1) != CrsNodesArray.Length)
                throw new ArgumentException("There must be exactly one more node than edge.");
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(CrsNodesArray != null);
            Contract.Invariant(CrsNodesArray.Length > 0);
            Contract.Invariant(OperationEdgesArray != null);
        }

        private ICrs[] CrsNodesArray { get; set; }
        private ICoordinateOperationInfo[] OperationEdgesArray { get; set; }

        public IEnumerable<ICrs> CoordinateReferenceSystems {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICrs>>() != null);
                return new ReadOnlyCollection<ICrs>(CrsNodesArray);
            }
        }

        public IEnumerable<ICoordinateOperationInfo> CoordinateOperations {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>() != null);
                return new ReadOnlyCollection<ICoordinateOperationInfo>(OperationEdgesArray);
            }
        }

        [Pure]
        public CoordinateOperationCrsPathInfo Append(ICrs node, ICoordinateOperationInfo edge) {
            if(node == null) throw new ArgumentNullException("node");
            if(edge == null) throw new ArgumentNullException("edge");
            Contract.Ensures(Contract.Result<CoordinateOperationCrsPathInfo>() != null);
            var nodes = new ICrs[CrsNodesArray.Length + 1];
            CrsNodesArray.CopyTo(nodes, 0);
            nodes[nodes.Length - 1] = node;
            var edges = new ICoordinateOperationInfo[OperationEdgesArray.Length + 1];
            OperationEdgesArray.CopyTo(edges, 0);
            edges[edges.Length - 1] = edge;
            return new CoordinateOperationCrsPathInfo(nodes, edges);
        }

        [Pure]
        public CoordinateOperationCrsPathInfo Prepend(ICrs node, ICoordinateOperationInfo edge) {
            if (node == null) throw new ArgumentNullException("node");
            if (edge == null) throw new ArgumentNullException("edge");
            Contract.Ensures(Contract.Result<CoordinateOperationCrsPathInfo>() != null);
            var nodes = new ICrs[CrsNodesArray.Length + 1];
            CrsNodesArray.CopyTo(nodes, 1);
            nodes[0] = node;
            var edges = new ICoordinateOperationInfo[OperationEdgesArray.Length + 1];
            OperationEdgesArray.CopyTo(edges, 1);
            edges[0] = edge;
            return new CoordinateOperationCrsPathInfo(nodes, edges);
        }

        [Pure]
        public CoordinateOperationCrsPathInfo Append(CoordinateOperationCrsPathInfo other) {
            Contract.Requires(other != null);
            Contract.Ensures(Contract.Result<CoordinateOperationCrsPathInfo>() != null);
            return Join(this, other);
        }

        public ICrs From {
            get { return CrsNodesArray[0]; }
        }

        public ICrs To {
            get { return CrsNodesArray[CrsNodesArray.Length-1]; }
        }
    }
}
