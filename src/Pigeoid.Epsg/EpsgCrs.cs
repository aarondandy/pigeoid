using Pigeoid.Epsg.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Pigeoid.Epsg
{
    public abstract class EpsgCrs : ICrs
    {

        internal const int Wgs84GeographicCode = 4326;

        [Obsolete]
        internal static readonly EpsgDataResourceReaderCrsNormal ReaderNormal = new EpsgDataResourceReaderCrsNormal();
        [Obsolete]
        internal static readonly EpsgDataResourceReaderCrsCompound ReaderCompound = new EpsgDataResourceReaderCrsCompound();

        [Obsolete]
        public static EpsgCrs Get(int code) {
            if (code <= 0)
                return null;
            
            var result = ReaderNormal.GetByKey(unchecked((uint)code));
            if(result != null)
                return result;

            if(code <= UInt16.MaxValue)
                return (EpsgCrs)(ReaderCompound.GetByKey(unchecked((ushort)code)));

            return null;
        }

        [Obsolete]
        public static IEnumerable<EpsgCrs> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCrs>>() != null);
                return ReaderNormal.ReadAllValues()
                    .Concat(ReaderCompound.ReadAllValues())
                    .OrderBy(x => x.Code);
            }
        }

        private readonly int _code;
        private readonly EpsgArea _area;
        private readonly bool _deprecated;

        internal EpsgCrs(int code, string name, EpsgArea area, bool deprecated) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            _code = code;
            Name = name;
            _area = area;
            _deprecated = deprecated;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(_code >= 0);
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(_area != null);
        }

        public int Code {
            get {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return _code;
            }
        }

        public string Name { get; private set; }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

        public EpsgArea Area {
            get {
                Contract.Ensures(Contract.Result<EpsgArea>() != null);
                return _area;
            }
        }

        public bool Deprecated { get { return _deprecated; } }

        public abstract EpsgCrsKind Kind { get; }

        public override string ToString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return Authority.ToString();
        }

    }
}
