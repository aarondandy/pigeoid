using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Pigeoid.Epsg
{
    public abstract class EpsgCrs : ICrs
    {

        internal const int Wgs84GeographicCode = 4326;

        public static EpsgCrs Get(int code) {
            return EpsgCrsDatumBased.GetDatumBased(code)
                ?? EpsgCrsProjected.GetProjected(code)
                ?? (EpsgCrs)(EpsgCrsCompound.GetCompound(code));
        }

        public static IEnumerable<EpsgCrs> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCrs>>() != null);
                return EpsgCrsDatumBased.DatumBasedValues
                    .Concat<EpsgCrs>(EpsgCrsProjected.ProjectedValues)
                    .Concat<EpsgCrs>(EpsgCrsCompound.CompoundValues)
                    .OrderBy(x => x.Code);
            }
        }

        private readonly int _code;
        private readonly EpsgArea _area;
        private readonly bool _deprecated;

        internal EpsgCrs(int code, string name, EpsgArea area, bool deprecated) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _code = code;
            Name = name;
            _area = area;
            _deprecated = deprecated;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
        }

        public int Code { get { return _code; } }

        public string Name { get; private set; }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

        public EpsgArea Area { get { return _area; } }

        public bool Deprecated { get { return _deprecated; } }

        public override string ToString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return Authority.ToString();
        }

    }
}
