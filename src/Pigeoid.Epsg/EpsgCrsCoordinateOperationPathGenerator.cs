using Pigeoid.CoordinateOperation;
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

            if (from.Kind == EpsgCrsKind.Compound || from.Kind == EpsgCrsKind.Engineering)
                throw new NotImplementedException(String.Format("Support for the from CRS kind {0} is not yet implemented.", from.Kind));
            if (to.Kind == EpsgCrsKind.Compound || to.Kind == EpsgCrsKind.Engineering)
                throw new NotImplementedException(String.Format("Support for the to CRS kind {0} is not yet implemented.", to.Kind));


            throw new NotImplementedException();
        }

    }
}
