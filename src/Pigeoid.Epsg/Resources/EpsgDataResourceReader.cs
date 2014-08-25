using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
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
        protected readonly int FileHeaderSize;
        protected readonly int RecordKeySize;
        protected readonly int RecordDataSize;
        protected readonly int RecordTotalSize;
        protected readonly EpsgTextLookUp TextLookup;

        protected EpsgDataResourceReaderBasic(string dataFileName, string textFileName, int recordDataSize)
            : this(dataFileName, new EpsgTextLookUp(textFileName), recordDataSize) { }

        protected EpsgDataResourceReaderBasic(string dataFileName, EpsgTextLookUp textLookup, int recordDataSize) {
            Contract.Requires(textLookup != null);
            DataFileName = dataFileName;
            FileHeaderSize = sizeof(ushort);
            RecordKeySize = sizeof(ushort);
            RecordDataSize = recordDataSize;
            RecordTotalSize = RecordKeySize + RecordDataSize;
            TextLookup = textLookup;
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
                //return GetByLinearSearch(targetKey, reader);
                return GetByBinarySearch(targetKey, count, reader);
            }
        }

        private TValue GetByBinarySearch(ushort targetKey, ushort count, BinaryReader reader) {
            Contract.Assume(count > 1);
            var baseSteam = reader.BaseStream;
            var searchIndexLow = 0;
            var searchIndexHigh = count - 1;
            while (searchIndexHigh >= searchIndexLow) {
                var searchAtIndex = (searchIndexLow + searchIndexHigh) / 2;
                baseSteam.Seek(FileHeaderSize + (searchAtIndex * RecordTotalSize), SeekOrigin.Begin); // seek to the key
                var localKey = reader.ReadUInt16();
                if (localKey == targetKey)
                    return ReadValue(localKey, reader);
                else if (localKey < targetKey)
                    searchIndexLow = searchAtIndex + 1;
                else
                    searchIndexHigh = searchAtIndex - 1;
            }
            return null;
        }

        private TValue GetByLinearSearch(ushort targetKey, BinaryReader reader) {
            var baseSteam = reader.BaseStream;
            while (baseSteam.Position < baseSteam.Length) {
                var key = reader.ReadUInt16();
                if (key == targetKey)
                    return ReadValue(key, reader);
                else
                    baseSteam.Seek(this.RecordDataSize, SeekOrigin.Current);
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
            var name = TextLookup.GetString(reader.ReadUInt16());
            var iso2 = EpsgTextLookUp.LookUpIsoString(key, "iso2.dat", 2); // TODO: optimize
            var iso3 = EpsgTextLookUp.LookUpIsoString(key, "iso3.dat", 3); // TODO: optimize
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

    internal sealed class EpsgDataResourceReaderOperationMethod : EpsgDataResourceReaderBasic<EpsgCoordinateOperationMethodInfo>
    {
        public EpsgDataResourceReaderOperationMethod() : base(
            "opmethod.dat",
            "opmethod.txt",
            (sizeof(ushort) + sizeof(byte))
        ) { }

        protected override EpsgCoordinateOperationMethodInfo ReadValue(ushort key, BinaryReader reader) {
            var canReverse = reader.ReadByte() == 'B';
            var name = TextLookup.GetString(reader.ReadUInt16());
            return new EpsgCoordinateOperationMethodInfo(key, name, canReverse);
        }
    }

    internal sealed class EpsgDataResourceReaderParameterInfo : EpsgDataResourceReaderBasic<EpsgParameterInfo>
    {
        public EpsgDataResourceReaderParameterInfo() : base(
            "parameters.dat",
            "parameters.txt",
            sizeof(ushort)
        ) { }

        protected override EpsgParameterInfo ReadValue(ushort key, BinaryReader reader) {
            var name = TextLookup.GetString(reader.ReadUInt16());
            Contract.Assume(!String.IsNullOrEmpty(name));
            return new EpsgParameterInfo(key, name);
        }

    }

    internal sealed class EpsgDataResourceReaderCoordinateSystem : EpsgDataResourceReaderBasic<EpsgCoordinateSystem>
    {
        public EpsgDataResourceReaderCoordinateSystem()
            : base(
                "coordsys.dat",
                "coordsys.txt",
                sizeof(ushort) + sizeof(byte)
            ) { }

        protected override EpsgCoordinateSystem ReadValue(ushort key, BinaryReader reader) {
            var typeData = reader.ReadByte();
            var name = TextLookup.GetString(reader.ReadUInt16());
            return new EpsgCoordinateSystem(
                key, name,
                dimension: typeData & 3,
                deprecated: 0 != (typeData & 128),
                csType: DecodeCsType(typeData)
            );
        }

        private static EpsgCoordinateSystemKind DecodeCsType(byte value) {
            switch (value & 0x70) {
                case 0x10: return EpsgCoordinateSystemKind.Cartesian;
                case 0x20: return EpsgCoordinateSystemKind.Ellipsoidal;
                case 0x30: return EpsgCoordinateSystemKind.Spherical;
                case 0x40: return EpsgCoordinateSystemKind.Vertical;
                default: return EpsgCoordinateSystemKind.None;
            }
        }

    }

    internal sealed class EpsgDataResourceReaderEllipsoid : EpsgDataResourceReaderBasic<EpsgEllipsoid>
    {

        private readonly EpsgNumberLookUp _numberLookup = new EpsgNumberLookUp();

        public EpsgDataResourceReaderEllipsoid() : base(
            "ellipsoids.dat",
            "ellipsoids.txt",
            (sizeof(ushort) * 3) + sizeof(byte)
        ) { }

        protected override EpsgEllipsoid ReadValue(ushort key, BinaryReader reader) {
            var semiMajorAxis = _numberLookup.Get(reader.ReadUInt16());
            var valueB = _numberLookup.Get(reader.ReadUInt16());
            var name = TextLookup.GetString(reader.ReadUInt16());
            Contract.Assume(!String.IsNullOrEmpty(name));
            var uom = EpsgUnit.Get(reader.ReadByte() + 9000);
            Contract.Assume(uom != null);
            return new EpsgEllipsoid(
                key, name, uom,
                (valueB == semiMajorAxis)
                    ? new Sphere(semiMajorAxis)
                : (valueB < semiMajorAxis / 10.0)
                    ? new SpheroidEquatorialInvF(semiMajorAxis, valueB) as ISpheroid<double>
                : new SpheroidEquatorialPolar(semiMajorAxis, valueB)
            );
        }
    }

    internal sealed class EpsgDataResourceReaderPrimeMeridian : EpsgDataResourceReaderBasic<EpsgPrimeMeridian>
    {

        private readonly EpsgNumberLookUp _numberLookup = new EpsgNumberLookUp();

        public EpsgDataResourceReaderPrimeMeridian() : base(
            "meridians.dat",
            "meridians.txt",
            (sizeof(ushort) * 2) + sizeof(byte)
        ) { }

        protected override EpsgPrimeMeridian ReadValue(ushort key, BinaryReader reader) {
            var uom = EpsgUnit.Get(reader.ReadUInt16());
            Contract.Assume(uom != null);
            var longitude = _numberLookup.Get(reader.ReadUInt16());
            var name = TextLookup.GetString(reader.ReadByte());
            Contract.Assume(!String.IsNullOrEmpty(name));
            return new EpsgPrimeMeridian(key, name, longitude, uom);
        }

    }

    internal sealed class EpsgDataResourceReaderBasicDatum<TValue> : EpsgDataResourceReaderBasic<TValue> where TValue : class
    {
        public readonly Func<ushort, string, EpsgArea, TValue> _construct;

        public EpsgDataResourceReaderBasicDatum(string dataFileName, Func<ushort, string, EpsgArea, TValue> construct)
            : base(
            dataFileName,
            "datums.txt",
            sizeof(ushort) * 2
        ) {
            Contract.Requires(construct != null);
            _construct = construct;
        }

        protected override TValue ReadValue(ushort key, BinaryReader reader) {
            var name = TextLookup.GetString(reader.ReadUInt16());
            var area = EpsgArea.Get(reader.ReadUInt16());
            return _construct(key, name, area);
        }
    }

    internal sealed class EpsgDataResourceReaderGeodeticDatum : EpsgDataResourceReaderBasic<EpsgDatumGeodetic>
    {
        public EpsgDataResourceReaderGeodeticDatum() : base(
            "datumgeo.dat",
            "datums.txt",
            sizeof(ushort) * 4
        ) { }

        protected override EpsgDatumGeodetic ReadValue(ushort key, BinaryReader reader) {
            var name = TextLookup.GetString(reader.ReadUInt16());
            Contract.Assume(!String.IsNullOrEmpty(name));
            var area = EpsgArea.Get(reader.ReadUInt16());
            Contract.Assume(area != null);
            var spheroid = EpsgEllipsoid.Get(reader.ReadUInt16());
            Contract.Assume(spheroid != null);
            var meridian = EpsgPrimeMeridian.Get(reader.ReadUInt16());
            Contract.Assume(meridian != null);
            return new EpsgDatumGeodetic(key, name, spheroid, meridian, area);
        }
    }

    internal sealed class EpsgDataResourceReaderAxisSet : EpsgDataResourceReaderBasic<EpsgAxisSet>
    {

        private readonly int Dimensions;

        public EpsgDataResourceReaderAxisSet(int dimensions, EpsgTextLookUp textLookup) : base(
            "axis" + dimensions.ToString(CultureInfo.InvariantCulture) + ".dat",
            textLookup,
            (4 * sizeof(ushort)) * dimensions
        ){
            Contract.Requires(dimensions > 0);
            Dimensions = dimensions;
        }

        protected override EpsgAxisSet ReadValue(ushort key, BinaryReader reader) {
            var axes = new EpsgAxis[Dimensions];
            for (int i = 0; i < axes.Length; ++i) {
                var unit = EpsgUnit.Get(reader.ReadUInt16());
                var name = TextLookup.GetString(reader.ReadUInt16());
                var orientation = TextLookup.GetString(reader.ReadUInt16());
                var abbr = TextLookup.GetString(reader.ReadUInt16());
                axes[i] = new EpsgAxis(name, abbr, orientation, unit);
            }
            return new EpsgAxisSet(key, axes);
        }

    }

    internal sealed class EpsgDataResourceAllAxisSetReaders
    {

        public EpsgDataResourceAllAxisSetReaders() {
            var textLookup = new EpsgTextLookUp("axis.txt");
            Dimension1 = new EpsgDataResourceReaderAxisSet(1, textLookup);
            Dimension2 = new EpsgDataResourceReaderAxisSet(2, textLookup);
            Dimension3 = new EpsgDataResourceReaderAxisSet(3, textLookup);
        }

        public EpsgDataResourceReaderAxisSet Dimension1 { get; private set; }
        public EpsgDataResourceReaderAxisSet Dimension2 { get; private set; }
        public EpsgDataResourceReaderAxisSet Dimension3 { get; private set; }

        public EpsgAxisSet GetSetByCsKey(ushort key) {
            return Dimension2.GetByKey(key)
                ?? Dimension3.GetByKey(key)
                ?? Dimension1.GetByKey(key);
        }

        [Obsolete]
        public IEnumerable<EpsgAxisSet> ReadAllValues() {
            return Dimension1.ReadAllValues()
                .Concat(Dimension2.ReadAllValues())
                .Concat(Dimension3.ReadAllValues());
        }

    }

    internal sealed class EpsgDataResourceReaderUnit : EpsgDataResourceReaderBasic<EpsgUnit>
    {

        private readonly EpsgNumberLookUp _numberLookup = new EpsgNumberLookUp();

        public EpsgDataResourceReaderUnit(string typeName) : base(
            "uom" + typeName.ToLowerInvariant() + ".dat",
            "uoms.txt",
            sizeof(ushort) * 3
        ) {
            Contract.Requires(typeName != null);
            TypeName = typeName;
        }

        public string TypeName { get; private set; }

        protected override EpsgUnit ReadValue(ushort key, BinaryReader reader) {
            var name = TextLookup.GetString(reader.ReadUInt16());
            Contract.Assume(!String.IsNullOrEmpty(name));
            var factorB = _numberLookup.Get(reader.ReadUInt16());
            var factorC = _numberLookup.Get(reader.ReadUInt16());
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (factorC == 0) {
                if (factorB == 0)
                    factorC = Double.NaN;
                else
                    throw new InvalidDataException("Bad unit conversion factor values.");
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
            return new EpsgUnit(key, name, TypeName, factorB, factorC);
        }

    }

    internal sealed class EpsgDataResourceAllUnitsReader
    {

        public EpsgDataResourceAllUnitsReader() {
            ReaderLength = new EpsgDataResourceReaderUnit("Length");
            ReaderAngle = new EpsgDataResourceReaderUnit("Angle");
            ReaderScale = new EpsgDataResourceReaderUnit("Scale");
            ReaderTime = new EpsgDataResourceReaderUnit("Time");
        }

        public EpsgDataResourceReaderUnit ReaderLength { get; private set; }
        public EpsgDataResourceReaderUnit ReaderAngle { get; private set; }
        public EpsgDataResourceReaderUnit ReaderScale { get; private set; }
        public EpsgDataResourceReaderUnit ReaderTime { get; private set; }

        public EpsgUnit GetByKey(ushort targetKey) {
            return ReaderLength.GetByKey(targetKey)
                ?? ReaderAngle.GetByKey(targetKey)
                ?? ReaderScale.GetByKey(targetKey)
                ?? ReaderTime.GetByKey(targetKey);
        }

        public IEnumerable<EpsgUnit> ReadAllValues() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgUnit>>() != null);
            return ReaderTime.ReadAllValues()
                .Concat(ReaderLength.ReadAllValues())
                .Concat(ReaderAngle.ReadAllValues())
                .Concat(ReaderScale.ReadAllValues())
                .OrderBy(x => x.Code);
        }
    }

    internal sealed class EpsgDataResourceReaderCrsNormal
    {

        private const int FileHeaderSize = sizeof(ushort);
        private const string DataFileName = "crs.dat";
        private const int RecordKeySize = sizeof(uint);
        private const int RecordDataSize = (sizeof(ushort) * 6) + (sizeof(byte) * 2);
        private const int RecordTotalSize = RecordKeySize + RecordDataSize;
        protected readonly EpsgTextLookUp TextLookup;

        public EpsgDataResourceReaderCrsNormal() {
            TextLookup = new EpsgTextLookUp("crs.txt");
        }

        protected EpsgCrs ReadValue(uint key, BinaryReader reader) {
            var datum = EpsgDatum.Get(reader.ReadUInt16());
            var baseCrs = EpsgCrs.Get(reader.ReadInt16());
            var baseOpCode = reader.ReadInt16();
            var coordSys = EpsgCoordinateSystem.Get(reader.ReadUInt16());
            var area = EpsgArea.Get(reader.ReadUInt16());
            var name = TextLookup.GetString(reader.ReadUInt16());
            var isDeprecated = reader.ReadByte() != 0;
            var kind = reader.ReadByte();

            Contract.Assume(kind != (byte)('C'));
            Contract.Assume(key <= Int32.MaxValue);

            if (kind == (byte)('V'))
                return new EpsgCrsVertical(unchecked((int)key), name, area, isDeprecated, coordSys, (EpsgDatumVertical)datum);
            if (kind == (byte)('E'))
                return new EpsgCrsEngineering(unchecked((int)key), name, area, isDeprecated, coordSys, (EpsgDatumEngineering)datum);

            throw new NotImplementedException();
        }

        public EpsgCrs GetByKey(uint targetKey) {
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DataFileName)) {
                var count = reader.ReadUInt16();
                //return GetByLinearSearch(targetKey, reader);
                return GetByBinarySearch(targetKey, count, reader);
            }
        }

        private EpsgCrs GetByBinarySearch(uint targetKey, ushort count, BinaryReader reader) {
            Contract.Assume(count > 1);
            var baseSteam = reader.BaseStream;
            var searchIndexLow = 0;
            var searchIndexHigh = count - 1;
            while (searchIndexHigh >= searchIndexLow) {
                var searchAtIndex = (searchIndexLow + searchIndexHigh) / 2;
                baseSteam.Seek(FileHeaderSize + (searchAtIndex * RecordTotalSize), SeekOrigin.Begin); // seek to the key
                var localKey = reader.ReadUInt32();
                if (localKey == targetKey)
                    return ReadValue(localKey, reader);
                else if (localKey < targetKey)
                    searchIndexLow = searchAtIndex + 1;
                else
                    searchIndexHigh = searchAtIndex - 1;
            }
            return null;
        }

        public IEnumerable<EpsgCrs> ReadAllValues() {
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DataFileName)) {
                var baseStream = reader.BaseStream;
                var count = reader.ReadUInt16();
                while (baseStream.Position < baseStream.Length) {
                    var key = reader.ReadUInt32();
                    var value = ReadValue(key, reader);
                    yield return value;
                }
            }
        }

    }

}
