using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{

    public class EpsgParameterInfo
    {

        internal class EpsgParameterInfoLookUp : EpsgDynamicLookUpBase<ushort, EpsgParameterInfo>
        {
            private const string DatFileName = "parameters.dat";
            private const string TxtFileName = "parameters.txt";
            private const int FileHeaderSize = sizeof(ushort);
            private const int RecordDataSize = sizeof(short);
            private const int RecordSize = sizeof(ushort) + RecordDataSize;
            private const int CodeSize = sizeof(ushort);

            private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

            private static ushort[] GetKeys() {
                Contract.Ensures(Contract.Result<ushort[]>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    var keys = new ushort[reader.ReadUInt16()];
                    for (int i = 0; i < keys.Length; i++) {
                        keys[i] = reader.ReadUInt16();
                        reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
                    }
                    return keys;
                }
            }

            public EpsgParameterInfoLookUp() : base(GetKeys()) { }

            protected override EpsgParameterInfo Create(ushort key, int index) {
                Contract.Ensures(Contract.Result<EpsgParameterInfo>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + FileHeaderSize + CodeSize, SeekOrigin.Begin);
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    Contract.Assume(!String.IsNullOrEmpty(name));
                    return new EpsgParameterInfo(key, name);
                }
            }

            protected override ushort GetKeyForItem(EpsgParameterInfo value) {
                return value._code;
            }
        }

        internal static readonly EpsgParameterInfoLookUp LookUp = new EpsgParameterInfoLookUp();

        public static EpsgParameterInfo Get(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? LookUp.Get(unchecked((ushort)code))
                : null;
        }

        public static IEnumerable<EpsgParameterInfo> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgParameterInfo>>() != null);
                return LookUp.Values;
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
