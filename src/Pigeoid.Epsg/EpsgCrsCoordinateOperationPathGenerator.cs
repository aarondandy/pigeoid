using Pigeoid.CoordinateOperation;
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

            var corePaths = FindAllCorePaths(startNode, to);

            throw new NotImplementedException();
        }

        private IEnumerable<EpsgCrsPathSearchNode> FindAllCorePaths(EpsgCrsPathSearchNode startNode, EpsgCrs targetCrs) {
            Contract.Requires(startNode != null);
            Contract.Requires(targetCrs != null);

            throw new NotImplementedException();
        }

    }
}
