using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vertesaur;

namespace Pigeoid.Epsg.Resources
{

    internal static class EpsgDataResourceReader
    {
        private static readonly Assembly ResourceAssembly = typeof(EpsgDataResourceReader).Assembly;
        private static readonly string ResourceBaseName = typeof(EpsgDataResourceReader).Namespace + ".";

        public static Stream CreateStream(string resourceName) {
            Contract.Ensures(Contract.Result<Stream>() != null);
            var stream = ResourceAssembly.GetManifestResourceStream(ResourceBaseName + resourceName);
            if (stream == null)
                throw new FileNotFoundException("Resource file not found: " + resourceName);
            return stream;
        }

        public static BinaryReader CreateBinaryReader(string resourceName) {
            Contract.Ensures(Contract.Result<BinaryReader>() != null);
            return new BinaryReader(CreateStream(resourceName));
        }
    }

    internal abstract class EpsgDataResourceReaderBasic<TValue> where TValue : class
    {

        protected readonly string DataFileName;
        protected readonly string TextFileName;
        protected readonly int FileHeaderSize;
        protected readonly int RecordKeySize;
        protected readonly int RecordDataSize;
        protected readonly int RecordTotalSize;

        protected EpsgDataResourceReaderBasic(string dataFileName, string textFileName, int recordDataSize) {
            DataFileName = dataFileName;
            TextFileName = textFileName;
            FileHeaderSize = sizeof(ushort);
            RecordKeySize = sizeof(ushort);
            RecordDataSize = recordDataSize;
            RecordTotalSize = RecordKeySize + RecordDataSize;
        }

        public IEnumerable<TValue> ReadAllValues() {
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DataFileName)) {
                var baseStream = reader.BaseStream;
                var count = reader.ReadUInt16();
                while (baseStream.Position < baseStream.Length) {
                    var key = reader.ReadUInt16();
                    var value = ReadValue(key, reader);
                    yield return value;
                }
            }
        }
        public TValue GetByKey(ushort targetKey) {
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DataFileName)) {
                var count = reader.ReadUInt16();
                return GetByBinarySearch(targetKey, count, reader);
            }
        }

        private TValue GetByBinarySearch(ushort targetKey, ushort count, BinaryReader reader) {
            Contract.Assume(count > 1);
            var baseSteam = reader.BaseStream;
            var searchIndexLow = 0;
            var searchIndexHigh = count - 1;
            while (searchIndexLow < searchIndexHigh) {
                var searchAtIndex = (searchIndexLow + searchIndexHigh) / 2;
                Contract.Assume(searchAtIndex < searchIndexHigh);

                // seek to the key
                baseSteam.Seek(FileHeaderSize + (searchAtIndex * RecordTotalSize), SeekOrigin.Begin);
                var localKey = reader.ReadUInt16();

                if (localKey == targetKey)
                    return ReadValue(localKey, reader);
                else if (localKey < targetKey)
                    searchIndexLow = searchAtIndex + 1;
                else
                    searchIndexHigh = searchAtIndex;
            }
            return null;
        }

        protected abstract TValue ReadValue(ushort key, BinaryReader reader);

    }

    internal sealed class EpsgDataResourceReaderArea : EpsgDataResourceReaderBasic<EpsgArea>
    {

        public EpsgDataResourceReaderArea() : base(
            "areas.dat",
            "areas.txt",
            (4 * sizeof(short)) + sizeof(ushort)
        ) { }

        protected override EpsgArea ReadValue(ushort key, BinaryReader reader) {
            var westBound = DecodeDegreeValueFromShort(reader.ReadInt16());
            var eastBound = DecodeDegreeValueFromShort(reader.ReadInt16());
            var southBound = DecodeDegreeValueFromShort(reader.ReadInt16());
            var northBound = DecodeDegreeValueFromShort(reader.ReadInt16());
            var nameCode = reader.ReadUInt16();
            var name = String.Empty; // TODO: read text
            var iso2 = (string)null; // TODO
            var iso3 = (string)null; // TODO
            return new EpsgArea(
                key,
                name,
                iso2,
                iso3,
                new LongitudeDegreeRange(westBound, eastBound),
                new Range(southBound, northBound)
            );
        }

        private static double DecodeDegreeValueFromShort(short encoded) {
            Contract.Ensures(!Double.IsNaN(Contract.Result<double>()));
            var v = encoded / 100.0;
            while (v < -180 || v > 180) {
                v /= 10.0;
            }
            return v;
        }
    }



}
