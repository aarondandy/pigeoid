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

        public ICoordinateOperationCrsPathInfo Generate(ICrs from, ICrs to) {
            var fromEpsg = from as EpsgCrs;
            var toEpsg = to as EpsgCrs;
            if (fromEpsg != null && toEpsg != null)
                return Generate(fromEpsg, toEpsg);
            throw new NotImplementedException();
        }

        public ICoordinateOperationCrsPathInfo Generate(EpsgCrs from, EpsgCrs to) {
            var allPaths = FinalAllPaths(from, to);
            return allPaths.FirstOrDefault();
        }

        private IEnumerable<ICoordinateOperationCrsPathInfo> FinalAllPaths(EpsgCrs from, EpsgCrs to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationCrsPathInfo>>() != null);
            var searcher = new EpsgCrsGraphSearcher(from, to);
            throw new NotImplementedException();
        }

    }
}
