using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;
using System.Collections.Generic;

namespace Pigeoid.Epsg
{
    public abstract class EpsgCrsDatumBased : EpsgCrs
    {

        internal class EpsgCrsDatumBasedLookUp : EpsgDynamicLookUpBase<int, EpsgCrsDatumBased>
        {

            private const string DatFileName = "crsgeo.dat";
            private const string TxtFileName = "crs.txt";
            private const int RecordDataSize = (sizeof(ushort) * 4) + (sizeof(byte) * 2);
            private const int CodeSize = sizeof(uint);
            private const int RecordSize = CodeSize + RecordDataSize;

            private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

            private static int[] GetKeys() {
                Contract.Ensures(Contract.Result<int[]>() != null);
                var keys = new List<int>();
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    while (reader.BaseStream.Position < reader.BaseStream.Length) {
                        keys.Add((int)reader.ReadUInt32());
                        reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
                    }
                }
                return keys.ToArray();
            }

            public EpsgCrsDatumBasedLookUp() : base(GetKeys()) { }

            protected override EpsgCrsDatumBased Create(int code, int index) {
                Contract.Ensures(Contract.Result<EpsgCrsDatumBased>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
                    var datum = EpsgDatum.Get(reader.ReadUInt16());
                    var cs = EpsgCoordinateSystem.Get(reader.ReadUInt16());
                    var area = EpsgArea.Get(reader.ReadUInt16());
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    var deprecated = reader.ReadByte() == 0xff;
                    var kind = reader.ReadByte();
                    switch (kind) {
                    case (byte)'3': // geographic3D
                    case (byte)'2': // geographic2D
                        return new EpsgCrsGeographic(code, name, area, deprecated, cs, (EpsgDatumGeodetic)datum);
                    case (byte)'G': // geocentric
                        return new EpsgCrsGeocentric(code, name, area, deprecated, cs, (EpsgDatumGeodetic)datum);
                    case (byte)'V': // vertical
                        return new EpsgCrsVertical(code, name, area, deprecated, cs, (EpsgDatumVertical)datum);
                    case (byte)'E': // engineering
                        return new EpsgCrsEngineering(code, name, area, deprecated, cs, (EpsgDatumEngineering)datum);
                    default:
                        throw new InvalidDataException();
                    }
                }
            }

            protected override int GetKeyForItem(EpsgCrsDatumBased value) {
                Contract.Requires(value != null);
                return value.Code;
            }

        }

        internal static readonly EpsgCrsDatumBasedLookUp LookUp = new EpsgCrsDatumBasedLookUp();

        public static EpsgCrsDatumBased GetDatumBased(int code) {
            return LookUp.Get(code);
        }

        public static IEnumerable<EpsgCrsDatumBased> DatumBasedValues {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCrsDatumBased>>() != null);
                return LookUp.Values;
            }
        }

        internal EpsgCrsDatumBased(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs)
            : base(code, name, area, deprecated) {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            CoordinateSystem = cs;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(CoordinateSystem != null);
        }

        public EpsgCoordinateSystem CoordinateSystem { get; private set; }

        public abstract EpsgDatum Datum { get; }

    }
}
