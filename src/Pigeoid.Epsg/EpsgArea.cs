using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Epsg
{
    public class EpsgArea :
        IRelatableIntersects<EpsgArea>,
        IRelatableContains<EpsgArea>,
        IRelatableWithin<EpsgArea>
    {

        internal class EpsgAreaLookUp : EpsgDynamicLookUpBase<ushort, EpsgArea>
        {

            private const string DatFileName = "areas.dat";
            private const string TxtFileName = "areas.txt";
            private const int FileHeaderSize = sizeof(ushort);
            private const int RecordDataSize = (4 * sizeof(short)) + sizeof(ushort);
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

            public EpsgAreaLookUp() : base(GetKeys()) { }

            private static double DecodeDegreeValueFromShort(short encoded) {
                Contract.Ensures(!Double.IsNaN(Contract.Result<double>()));
                var v = encoded / 100.0;
                while (v < -180 || v > 180) {
                    v /= 10.0;
                }
                return v;
            }

            protected override EpsgArea Create(ushort key, int index) {
                Contract.Ensures(Contract.Result<EpsgArea>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + FileHeaderSize + CodeSize, SeekOrigin.Begin);
                    var westBound = DecodeDegreeValueFromShort(reader.ReadInt16());
                    var eastBound = DecodeDegreeValueFromShort(reader.ReadInt16());
                    var southBound = DecodeDegreeValueFromShort(reader.ReadInt16());
                    var northBound = DecodeDegreeValueFromShort(reader.ReadInt16());
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    return new EpsgArea(
                        key, name,
                        EpsgTextLookUp.LookUpIsoString(key, "iso2.dat", 2),
                        EpsgTextLookUp.LookUpIsoString(key, "iso3.dat", 3),
                        new LongitudeDegreeRange(westBound, eastBound),
                        new Range(southBound, northBound)
                    );
                }
            }

            protected override ushort GetKeyForItem(EpsgArea value) {
                Contract.Requires(value != null);
                return (ushort)(value.Code);
            }

        }

        internal static readonly EpsgAreaLookUp LookUp = new EpsgAreaLookUp();

        public static EpsgArea Get(int code) {
            Contract.Ensures(Contract.Result<EpsgArea>() != null);
            return LookUp.Get(checked((ushort)code));
        }

        public static IEnumerable<EpsgArea> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgArea>>() != null);
                return LookUp.Values;
            }
        }

        internal EpsgArea(ushort code, string name, string iso2, string iso3, LongitudeDegreeRange longitudeRange, Range latRange) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(!String.IsNullOrEmpty(iso2));
            Contract.Requires(!String.IsNullOrEmpty(iso3));
            Code = code;
            Name = name;
            Iso2 = iso2;
            Iso3 = iso3;
            LongitudeRange = longitudeRange;
            LatitudeRange = latRange;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(!String.IsNullOrEmpty(Iso2));
            Contract.Invariant(!String.IsNullOrEmpty(Iso3));
        }

        public int Code { get; private set; }

        public string Name { get; private set; }

        public string Iso2 { get; private set; }

        public string Iso3 { get; private set; }

        public LongitudeDegreeRange LongitudeRange { get; private set; }

        public Range LatitudeRange { get; private set; }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(Code);
            }
        }

        public bool Intersects(EpsgArea other) {
            Contract.Requires(other != null);
            return LongitudeRange.Intersects(other.LongitudeRange)
                && LatitudeRange.Intersects(other.LatitudeRange);
        }

        public bool Contains(EpsgArea other) {
            Contract.Requires(other != null);
            return LongitudeRange.Contains(other.LongitudeRange)
                && LatitudeRange.Contains(other.LatitudeRange);
        }

        public bool Within(EpsgArea other) {
            Contract.Requires(other != null);
            return LongitudeRange.Within(other.LongitudeRange)
                && LatitudeRange.Within(other.LatitudeRange);
        }

    }
}
