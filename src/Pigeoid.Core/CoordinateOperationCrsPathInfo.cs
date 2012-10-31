using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class CoordinateOperationCrsPathInfo : ICoordinateOperationCrsPathInfo
	{

		private static readonly ReadOnlyCollection<ICoordinateOperationInfo> EmptyEdgeList = Array.AsReadOnly(new ICoordinateOperationInfo[0]);

		public static CoordinateOperationCrsPathInfo Join(CoordinateOperationCrsPathInfo a, CoordinateOperationCrsPathInfo b) {
			// TODO: make sure that the last node of the first list and the first node of the last list are a match
			var nodes = new ICrs[a.CrsNodes.Count + b.CrsNodes.Count - 1];
			a.CrsNodes.CopyTo(nodes, 0);
			b.CrsNodes.CopyTo(nodes, a.CrsNodes.Count-1);
			var edges = new ICoordinateOperationInfo[a.OpEdges.Count + b.OpEdges.Count];
			a.OpEdges.CopyTo(edges, 0);
			b.OpEdges.CopyTo(edges, a.OpEdges.Count);
			return new CoordinateOperationCrsPathInfo(nodes, edges);
		}

		private readonly ReadOnlyCollection<ICrs> CrsNodes;
		private readonly ReadOnlyCollection<ICoordinateOperationInfo> OpEdges;
 
		public CoordinateOperationCrsPathInfo(ICrs node) {
			CrsNodes = Array.AsReadOnly(new[]{node});
			OpEdges = EmptyEdgeList;
		}

		public CoordinateOperationCrsPathInfo(IEnumerable<ICrs> nodes, IEnumerable<ICoordinateOperationInfo> edges)
			: this(nodes.ToArray(), edges.ToArray())
		{ }

		private CoordinateOperationCrsPathInfo(ICrs[] nodes, ICoordinateOperationInfo[] edges) {
			CrsNodes = Array.AsReadOnly(nodes);
			OpEdges = Array.AsReadOnly(edges);
			if ((OpEdges.Count + 1) != CrsNodes.Count)
				throw new ArgumentException("There must be exactly one more node than edge.");
		}

		public IEnumerable<ICrs> CoordinateReferenceSystems {
			get { return CrsNodes; }
		}

		public IEnumerable<ICoordinateOperationInfo> CoordinateOperations {
			get { return OpEdges; }
		}

		[Pure]
		public CoordinateOperationCrsPathInfo Append(ICrs node, ICoordinateOperationInfo edge) {
			var nodes = new ICrs[CrsNodes.Count + 1];
			CrsNodes.CopyTo(nodes,0);
			nodes[nodes.Length - 1] = node;
			var edges = new ICoordinateOperationInfo[OpEdges.Count + 1];
			OpEdges.CopyTo(edges, 0);
			edges[edges.Length - 1] = edge;
			return new CoordinateOperationCrsPathInfo(nodes,edges);
		}

		[Pure]
		public CoordinateOperationCrsPathInfo Prepend(ICrs node, ICoordinateOperationInfo edge) {
			var nodes = new ICrs[CrsNodes.Count + 1];
			CrsNodes.CopyTo(nodes, 1);
			nodes[0] = node;
			var edges = new ICoordinateOperationInfo[OpEdges.Count + 1];
			OpEdges.CopyTo(edges, 1);
			edges[0] = edge;
			return new CoordinateOperationCrsPathInfo(nodes, edges);
		}

		[Pure]
		public CoordinateOperationCrsPathInfo Append(CoordinateOperationCrsPathInfo other) {
			return Join(this, other);
		}
	}
}
