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
                return new PathNode { Crs = crs };
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

        public IEnumerable<ICoordinateOperationCrsPathInfo> FinalAllPaths(ICrs from, ICrs to) {
            var fromEpsg = from as EpsgCrs;
            var toEpsg = to as EpsgCrs;
            if (fromEpsg != null && toEpsg != null)
                return FinalAllPaths(fromEpsg, toEpsg);
            throw new NotImplementedException();
        }

        public IEnumerable<ICoordinateOperationCrsPathInfo> FinalAllPaths(EpsgCrs from, EpsgCrs to) {
            var startPathNode = PathNode.CreateStartNode(from);


            throw new NotImplementedException();
        }

    }
}
