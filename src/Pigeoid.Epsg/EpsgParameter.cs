using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{

    public class EpsgParameterInfo
    {

        [Obsolete]
        internal static readonly EpsgDataResourceReaderParameterInfo Reader = new EpsgDataResourceReaderParameterInfo();

        [Obsolete]
        public static EpsgParameterInfo Get(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? Reader.GetByKey(unchecked((ushort)code))
                : null;
        }

        [Obsolete]
        public static IEnumerable<EpsgParameterInfo> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgParameterInfo>>() != null);
                return Reader.ReadAllValues();
            }
        }

        private readonly ushort _code;

        internal EpsgParameterInfo(ushort code, string name) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _code = code;
            Name = name;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
        }

        public int Code { get { return _code; } }

        public string Name { get; private set; }

    }

}
