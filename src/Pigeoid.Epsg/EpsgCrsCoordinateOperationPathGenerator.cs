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
            var crsFilters = CrsFilters == null ? new List<Predicate<EpsgCrs>>() : CrsFilters.ToList();
            var opFilters = OpFilters == null ? new List<Predicate<ICoordinateOperationInfo>>() : OpFilters.ToList();
            var searcher = new EpsgCrsGraphSearcher(from, to) {
                CrsFilters = crsFilters,
                OpFilters = OpFilters
            };

            searcher.CrsFilters.Add(searcher.AreaContainsTest); // TODO: need a better way to handle this

            var allPaths = searcher.FindAllPaths();
            return allPaths;
        }

    }
}
