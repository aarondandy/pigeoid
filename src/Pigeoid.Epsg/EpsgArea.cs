using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Core;
using Pigeoid.Epsg.Resources;
using Vertesaur;

namespace Pigeoid.Epsg
{
    public class EpsgArea :
        IGeographicMbr,
        IRelatableIntersects<EpsgArea>,
        IRelatableContains<EpsgArea>,
        IRelatableWithin<EpsgArea>
    {

        [Obsolete]
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
                    Contract.Assume(!String.IsNullOrEmpty(name));
                    var iso2 = EpsgTextLookUp.LookUpIsoString(key, "iso2.dat", 2);
                    var iso3 = EpsgTextLookUp.LookUpIsoString(key, "iso3.dat", 3);
                    return new EpsgArea(
                        key, name,
                        iso2,
                        iso3,
                        new LongitudeDegreeRange(westBound, eastBound),
                        new Range(southBound, northBound)
                    );
                }
            }

            protected override ushort GetKeyForItem(EpsgArea value) {
                return (ushort)(value.Code);
            }

        }

        [Obsolete]
        internal static readonly EpsgAreaLookUp LookUp = new EpsgAreaLookUp();

        [Obsolete]
        public static EpsgArea Get(int code) {
            if (code < 0 || code >= UInt16.MaxValue)
                return null;
            return LookUp.Get(unchecked((ushort)code));
        }


        [Obsolete]
        public static IEnumerable<EpsgArea> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgArea>>() != null);
                return LookUp.Values;
            }
        }

        internal EpsgArea(ushort code, string name, string iso2, string iso3, LongitudeDegreeRange longitudeRange, Range latRange) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Code = code;
            Name = name;
            Iso2 = iso2;
            Iso3 = iso3;
            LongitudeRange = longitudeRange;
            LatitudeRange = latRange;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Code >= 0);
            Contract.Invariant(!String.IsNullOrEmpty(Name));
        }

        public int Code { get; private set; }

        public string Name { get; private set; }

        public string Iso2 { get; private set; }

        public string Iso3 { get; private set; }

        public LongitudeDegreeRange LongitudeRange { get; private set; }

        IPeriodicRange<double> IGeographicMbr.LongitudeRange { get{ return LongitudeRange; } }

        public Range LatitudeRange { get; private set; }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(Code);
            }
        }

        public bool Intersects(IGeographicMbr other) {
            return LongitudeRange.Intersects(other.LongitudeRange)
                && LatitudeRange.Intersects(other.LatitudeRange);
        }

        public bool Intersects(EpsgArea other) {
            return LongitudeRange.Intersects(other.LongitudeRange)
                && LatitudeRange.Intersects(other.LatitudeRange);
        }

        public bool Contains(EpsgArea other) {
            return LongitudeRange.Contains(other.LongitudeRange)
                && LatitudeRange.Contains(other.LatitudeRange);
        }

        public bool Within(EpsgArea other) {
            return LongitudeRange.Within(other.LongitudeRange)
                && LatitudeRange.Within(other.LatitudeRange);
        }

        [Obsolete("A more generic replacement should be used.")]
        private class IntersectionResult : IGeographicMbr
        {

            public IPeriodicRange<double> LongitudeRange { get; set; }

            public Range LatitudeRange { get; set; }

            public bool Intersects(IGeographicMbr other) {
                return LongitudeRange.Intersects(other.LongitudeRange)
                && LatitudeRange.Intersects(other.LatitudeRange);
            }

            public IGeographicMbr Intersection(IGeographicMbr other) {
                if (!LatitudeRange.Intersects(other.LatitudeRange))
                    return null;

                if (LongitudeRange == null)
                    return null;

                var longitude = LongitudeRange.Intersection(other.LongitudeRange);
                if (longitude == null)
                    return null;

                var latitude = new Range(
                    Math.Max(LatitudeRange.Low, other.LatitudeRange.Low),
                    Math.Min(LatitudeRange.High, other.LatitudeRange.High));

                return new IntersectionResult {
                    LatitudeRange = latitude,
                    LongitudeRange = longitude
                };
            }
        }

        public IGeographicMbr Intersection(IGeographicMbr other) {
            if (!LatitudeRange.Intersects(other.LatitudeRange))
                return null;

            var longitude = LongitudeRange.Intersection(other.LongitudeRange);
            if(longitude == null)
                return null;

            var latitude = new Range(
                Math.Max(LatitudeRange.Low, other.LatitudeRange.Low),
                Math.Min(LatitudeRange.High, other.LatitudeRange.High));

            return new IntersectionResult {
                LatitudeRange = latitude,
                LongitudeRange = longitude
            };
        }

    }
}
