using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
    public class EpsgCrsCompound : EpsgCrs, ICrsCompound
    {

        internal class EpsgCrsCompoundLookUp : EpsgDynamicLookUpBase<ushort, EpsgCrsCompound>
        {

            private const string DatFileName = "crscmp.dat";
            private const string TxtFileName = "crs.txt";
            private const int RecordDataSize = (sizeof(ushort) * 4) + sizeof(byte);
            private const int CodeSize = sizeof(ushort);
            private const int RecordSize = CodeSize + RecordDataSize;

            private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

            private static ushort[] GetKeys() {
                Contract.Ensures(Contract.Result<ushort[]>() != null);
                var keys = new List<ushort>();
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    while (reader.BaseStream.Position < reader.BaseStream.Length) {
                        keys.Add(reader.ReadUInt16());
                        reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
                    }
                }
                return keys.ToArray();
            }

            public EpsgCrsCompoundLookUp() : base(GetKeys()) { }

            protected override EpsgCrsCompound Create(ushort code, int index) {
                Contract.Ensures(Contract.Result<EpsgCrsCompound>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
                    var horizontal = EpsgCrs.Get(reader.ReadUInt16());
                    Contract.Assume(horizontal != null);
                    var vertical = (EpsgCrsVertical)EpsgCrsDatumBased.GetDatumBased(reader.ReadUInt16());
                    Contract.Assume(vertical != null);
                    var area = EpsgArea.Get(reader.ReadUInt16());
                    Contract.Assume(area != null);
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    Contract.Assume(!String.IsNullOrEmpty(name));
                    var deprecated = reader.ReadByte() == 0xff;
                    return new EpsgCrsCompound(code, name, area, deprecated, horizontal, vertical);
                }
            }

            protected override ushort GetKeyForItem(EpsgCrsCompound value) {
                return (ushort)value.Code;
            }

        }

        internal static readonly EpsgCrsCompoundLookUp LookUp = new EpsgCrsCompoundLookUp();

        public static EpsgCrsCompound GetCompound(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? LookUp.Get((ushort)code)
                : null;
        }

        public static IEnumerable<EpsgCrsCompound> CompoundValues {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCrsCompound>>() != null);
                return LookUp.Values;
            }
        }

        private EpsgCrsCompound(
            int code, string name, EpsgArea area, bool deprecated,
            EpsgCrs horizontal, EpsgCrsVertical vertical
        )
            : base(code, name, area, deprecated) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(horizontal != null);
            Contract.Requires(vertical != null);
            Horizontal = horizontal;
            Vertical = vertical;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Horizontal != null);
            Contract.Invariant(Vertical != null);
        }

        public EpsgCrs Horizontal { get; private set; }

        ICrs ICrsCompound.Head { get { return Horizontal; } }

        public EpsgCrsVertical Vertical { get; private set; }

        ICrs ICrsCompound.Tail { get { return Vertical; } }

        public override EpsgCrsKind Kind {
            get { return EpsgCrsKind.Compound; }
        }

    }
}
