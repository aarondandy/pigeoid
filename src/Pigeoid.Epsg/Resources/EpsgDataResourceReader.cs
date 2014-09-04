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

        [Obsolete]
        public static Stream CreateStream(string resourceName) {
            Contract.Ensures(Contract.Result<Stream>() != null);
            var stream = ResourceAssembly.GetManifestResourceStream(ResourceBaseName + resourceName);
            if (stream == null)
                throw new FileNotFoundException("Resource file not found: " + resourceName);
            return stream;
        }

        [Obsolete]
        public static BinaryReader CreateBinaryReader(string resourceName) {
            Contract.Ensures(Contract.Result<BinaryReader>() != null);
            return new BinaryReader(CreateStream(resourceName));
        }
    }

    internal class EpsgDataResourceReaderNumbers
    {

        internal static readonly EpsgDataResourceReaderNumbers Default = new EpsgDataResourceReaderNumbers();

        private const string DoubleFileName = "numbersd.dat";
        private const string IntFileName = "numbersi.dat";
        private const string ShortFileName = "numberss.dat";

        private readonly double[] _preloadDouble;
        private readonly int[] _preloadInt;
        private readonly short[] _preloadShort;


        private EpsgDataResourceReaderNumbers() {
            _preloadDouble = new double[8];
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DoubleFileName)) {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                for (var i = 0; i < _preloadDouble.Length; ++i) {
                    _preloadDouble[i] = reader.ReadDouble();
                }
            }

            _preloadInt = new int[16];
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(IntFileName)) {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                for (var i = 0; i < _preloadInt.Length; ++i) {
                    _preloadInt[i] = reader.ReadInt32();
                }
            }

            _preloadShort = new short[32];
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(ShortFileName)) {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                for (var i = 0; i < _preloadShort.Length; ++i) {
                    _preloadShort[i] = reader.ReadInt16();
                }
            }
        }

        public double GetValue(ushort code) {
            var index = (ushort)(code & 0x3fff);
            if ((code & 0xc000) == 0xc000)
                return GetInt(index);
            else if ((code & 0x4000) == 0x4000)
                return GetShort(index);
            return GetDouble(index);
        }

        private double GetDouble(ushort index) {
            if (index < _preloadDouble.Length)
                return _preloadDouble[index];

            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DoubleFileName)) {
                reader.BaseStream.Seek(index * sizeof(double), SeekOrigin.Begin);
                return reader.ReadDouble();
            }
        }

        private int GetInt(ushort index) {
            if (index < _preloadInt.Length)
                return _preloadInt[index];

            using (var reader = EpsgDataResourceReader.CreateBinaryReader(IntFileName)) {
                reader.BaseStream.Seek(index * sizeof(int), SeekOrigin.Begin);
                return reader.ReadInt32();
            }
        }

        private short GetShort(ushort index) {
            if (index < _preloadShort.Length)
                return _preloadShort[index];

            using (var reader = EpsgDataResourceReader.CreateBinaryReader(ShortFileName)) {
                reader.BaseStream.Seek(index * sizeof(short), SeekOrigin.Begin);
                return reader.ReadInt16();
            }
        } 


    }

    internal class EpsgDataResourceReaderText
    {

        private static string[] _preload;

        static EpsgDataResourceReaderText() {
            _preload = new string[64];
            using (var datReader = EpsgDataResourceReader.CreateBinaryReader("words.dat"))
            using (var txtReader = EpsgDataResourceReader.CreateBinaryReader("words.txt")) {
                for (int i = 0; i < _preload.Length; ++i) {
                    var word = ReadWord(i, datReader, txtReader);
                    _preload[i] = String.IsInterned(word) ?? word;
                }
            }
        }

        private static int[] Read7BitArray(BinaryReader reader) {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<int[]>() != null);
            var dataCount = reader.ReadByte();
            return Read7BitArray(dataCount, reader);
        }

        private static int[] Read7BitArray(int count, BinaryReader reader) {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<int[]>() != null);
            var resultBuilder = new List<int>(count);
            var data = reader.ReadBytes(count);
            int wordIndex = 0;
            int shift = 0;
            for (int i = 0; i < data.Length; ++i) {
                byte b = data[i];
                unchecked {
                    wordIndex |= (b & 0x7F) << shift;
                }
                if ((b & 0x80) == 0) {
                    resultBuilder.Add(wordIndex);
                    wordIndex = 0;
                    shift = 0;
                }
                else {
                    shift += 7;
                }
            }
            return resultBuilder.ToArray();
        }

        private static string BuildWordString(int[] wordIndices) {
            Contract.Requires(wordIndices != null);
            using (var datReader = EpsgDataResourceReader.CreateBinaryReader("words.dat"))
            using (var txtReader = EpsgDataResourceReader.CreateBinaryReader("words.txt")) {
                Contract.Assume(wordIndices.Length != 0);
                if (wordIndices.Length == 1)
                    return GetWord(wordIndices[0], datReader, txtReader);

                var words = Array.ConvertAll(wordIndices, wordIndex => GetWord(wordIndex, datReader, txtReader));
                return String.Concat(words);
            }
        }

        private static string GetWord(int wordIndex, BinaryReader datReader, BinaryReader txtReader) {
            Contract.Requires(datReader != null);
            Contract.Requires(txtReader != null);

            if (wordIndex < _preload.Length)
                return _preload[wordIndex];
            return ReadWord(wordIndex, datReader, txtReader);
        }

        private static string ReadWord(int wordIndex, BinaryReader datReader, BinaryReader txtReader) {
            Contract.Requires(datReader != null);
            Contract.Requires(txtReader != null);
            datReader.BaseStream.Seek(wordIndex * (sizeof(ushort) + sizeof(byte)), SeekOrigin.Begin);
            txtReader.BaseStream.Seek(datReader.ReadUInt16(), SeekOrigin.Begin);
            return Encoding.UTF8.GetString(txtReader.ReadBytes(datReader.ReadByte()));
        }

        private readonly string _wordPointerFile;

        public EpsgDataResourceReaderText(string wordPointerFile) {
            Contract.Requires(wordPointerFile != null);
            _wordPointerFile = wordPointerFile;
        }

        public string GetString(ushort stringOffset) {
            if (stringOffset == UInt16.MaxValue)
                return String.Empty;

            using (var pointerReader = EpsgDataResourceReader.CreateBinaryReader(_wordPointerFile)) {
                return GetString(stringOffset, pointerReader);
            }
        }

        private string GetString(ushort stringOffset, BinaryReader pointerReader) {
            Contract.Requires(pointerReader != null);
            Contract.Requires(stringOffset != UInt16.MaxValue);
            pointerReader.BaseStream.Seek(stringOffset, SeekOrigin.Begin);
            var wordIndices = Read7BitArray(pointerReader);
            return BuildWordString(wordIndices);
        }

    }

    internal abstract class EpsgDataResourceReaderBasic<TValue> where TValue : class
    {

        protected readonly string DataFileName;
        protected readonly int FileHeaderSize;
        protected readonly int RecordKeySize;
        protected readonly int RecordDataSize;
        protected readonly int RecordTotalSize;
        protected readonly EpsgDataResourceReaderText TextReader;
        private ushort _count = 0;


        [Obsolete]
        protected EpsgDataResourceReaderBasic(string dataFileName, string textFileName, int recordDataSize)
            : this(dataFileName, new EpsgDataResourceReaderText(textFileName), recordDataSize) { }

        protected EpsgDataResourceReaderBasic(string dataFileName, EpsgDataResourceReaderText textReader, int recordDataSize) {
            Contract.Requires(textReader != null);
            DataFileName = dataFileName;
            FileHeaderSize = sizeof(ushort);
            RecordKeySize = sizeof(ushort);
            RecordDataSize = recordDataSize;
            RecordTotalSize = RecordKeySize + RecordDataSize;
            TextReader = textReader;
        }

        public IEnumerable<TValue> ReadAllValues() {
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DataFileName)) {
                var baseStream = reader.BaseStream;
                if (_count == 0) {
                    baseStream.Seek(0, SeekOrigin.Begin);
                    _count = reader.ReadUInt16();
                }
                else {
                    baseStream.Seek(sizeof(ushort), SeekOrigin.Begin);
                }

                for(ushort i = 0; i < _count; ++i){
                    var key = reader.ReadUInt16();
                    var value = ReadValue(key, reader);
                    yield return value;
                }
            }
        }

        public TValue GetByKey(ushort targetKey) {
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DataFileName)) {
                if (_count == 0) {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    _count = reader.ReadUInt16();
                }

                /*if (_count <= 8) {
                    return GetByLinearSearch(targetKey, reader);
                }*/
                return GetByBinarySearch(targetKey, reader);
            }
        }

        private TValue GetByBinarySearch(ushort targetKey, BinaryReader reader) {
            Contract.Requires(_count != 0);
            var baseStream = reader.BaseStream;
            var searchIndexLow = 0;
            var searchIndexHigh = _count - 1;
            while (searchIndexHigh >= searchIndexLow) {
                var searchAtIndex = (searchIndexLow + searchIndexHigh) / 2;
                baseStream.Seek(FileHeaderSize + (searchAtIndex * RecordTotalSize), SeekOrigin.Begin); // seek to the key
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
            Contract.Requires(_count != 0);
            var baseStream = reader.BaseStream;
            baseStream.Seek(sizeof(ushort), SeekOrigin.Begin);
            for(ushort i = 0; i < _count; ++i) {
                var key = reader.ReadUInt16();
                if (key == targetKey)
                    return ReadValue(key, reader);
                else
                    baseStream.Seek(RecordDataSize, SeekOrigin.Current);
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
            (4 * sizeof(short)) + sizeof(ushort) + (sizeof(byte) * 5)
        ) { }

        private string ReadIsoBytes(BinaryReader reader, int n) {
            var data = reader.ReadBytes(n);
            Contract.Assert(data != null);
            Contract.Assert(data.Length == n);
            if (data[0] == 0)
                return null;
            return Encoding.ASCII.GetString(data);
        }

        protected override EpsgArea ReadValue(ushort key, BinaryReader reader) {
            var westBound = DecodeDegreeValueFromShort(reader.ReadInt16());
            var eastBound = DecodeDegreeValueFromShort(reader.ReadInt16());
            var southBound = DecodeDegreeValueFromShort(reader.ReadInt16());
            var northBound = DecodeDegreeValueFromShort(reader.ReadInt16());
            var name = TextReader.GetString(reader.ReadUInt16());
            var iso2 = ReadIsoBytes(reader, 2);
            var iso3 = ReadIsoBytes(reader, 3);
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
            var name = TextReader.GetString(reader.ReadUInt16());
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
            var name = TextReader.GetString(reader.ReadUInt16());
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
            var name = TextReader.GetString(reader.ReadUInt16());
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

        private readonly EpsgDataResourceReaderNumbers _numberReader = EpsgDataResourceReaderNumbers.Default;

        public EpsgDataResourceReaderEllipsoid() : base(
            "ellipsoids.dat",
            "ellipsoids.txt",
            (sizeof(ushort) * 3) + sizeof(byte)
        ) { }

        protected override EpsgEllipsoid ReadValue(ushort key, BinaryReader reader) {
            var semiMajorAxis = _numberReader.GetValue(reader.ReadUInt16());
            var valueB = _numberReader.GetValue(reader.ReadUInt16());
            var name = TextReader.GetString(reader.ReadUInt16());
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

        private readonly EpsgDataResourceReaderNumbers _numberReader = EpsgDataResourceReaderNumbers.Default;

        public EpsgDataResourceReaderPrimeMeridian() : base(
            "meridians.dat",
            "meridians.txt",
            (sizeof(ushort) * 2) + sizeof(byte)
        ) { }

        protected override EpsgPrimeMeridian ReadValue(ushort key, BinaryReader reader) {
            var uom = EpsgUnit.Get(reader.ReadUInt16());
            Contract.Assume(uom != null);
            var longitude = _numberReader.GetValue(reader.ReadUInt16());
            var name = TextReader.GetString(reader.ReadByte());
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
            var name = TextReader.GetString(reader.ReadUInt16());
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
            var name = TextReader.GetString(reader.ReadUInt16());
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

        public EpsgDataResourceReaderAxisSet(int dimensions, EpsgDataResourceReaderText textReader)
            : base(
            "axis" + dimensions.ToString(CultureInfo.InvariantCulture) + ".dat",
            textReader,
            (4 * sizeof(ushort)) * dimensions
        ){
            Contract.Requires(dimensions > 0);
            Dimensions = dimensions;
        }

        protected override EpsgAxisSet ReadValue(ushort key, BinaryReader reader) {
            var axes = new EpsgAxis[Dimensions];
            for (int i = 0; i < axes.Length; ++i) {
                var unit = EpsgUnit.Get(reader.ReadUInt16());
                var name = TextReader.GetString(reader.ReadUInt16());
                var orientation = TextReader.GetString(reader.ReadUInt16());
                var abbr = TextReader.GetString(reader.ReadUInt16());
                axes[i] = new EpsgAxis(name, abbr, orientation, unit);
            }
            return new EpsgAxisSet(key, axes);
        }

    }

    internal sealed class EpsgDataResourceAllAxisSetReaders
    {

        public EpsgDataResourceAllAxisSetReaders() {
            var textReader = new EpsgDataResourceReaderText("axis.txt");
            Dimension1 = new EpsgDataResourceReaderAxisSet(1, textReader);
            Dimension2 = new EpsgDataResourceReaderAxisSet(2, textReader);
            Dimension3 = new EpsgDataResourceReaderAxisSet(3, textReader);
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

        private readonly EpsgDataResourceReaderNumbers _numberReader = EpsgDataResourceReaderNumbers.Default;

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
            var name = TextReader.GetString(reader.ReadUInt16());
            Contract.Assume(!String.IsNullOrEmpty(name));
            var factorB = _numberReader.GetValue(reader.ReadUInt16());
            var factorC = _numberReader.GetValue(reader.ReadUInt16());
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
        protected readonly EpsgDataResourceReaderText TextReader;
        private ushort _count = 0;

        public EpsgDataResourceReaderCrsNormal() {
            TextReader = new EpsgDataResourceReaderText("crs.txt");
        }

        private EpsgCrs ReadValue(uint key, BinaryReader reader) {
            var datum = EpsgDatum.Get(reader.ReadUInt16());
            var baseCrs = (EpsgCrsGeodetic)EpsgCrs.Get(reader.ReadInt16());
            var baseOpCode = reader.ReadInt16();
            var coordSys = EpsgCoordinateSystem.Get(reader.ReadUInt16());
            var area = EpsgArea.Get(reader.ReadUInt16());
            var name = TextReader.GetString(reader.ReadUInt16());
            var isDeprecated = reader.ReadByte() != 0;
            var kind = (EpsgCrsKind)reader.ReadByte();

            Contract.Assume(kind != EpsgCrsKind.Compound);
            Contract.Assume(key <= Int32.MaxValue);

            var epsgCode = unchecked((int)key);

            if (kind == EpsgCrsKind.Projected)
                return new EpsgCrsProjected(epsgCode, name, area, isDeprecated, coordSys, (EpsgDatumGeodetic)datum, baseCrs, baseOpCode);

            if (kind == EpsgCrsKind.Geographic2D || kind == EpsgCrsKind.Geographic3D)
                return new EpsgCrsGeographic(epsgCode, name, area, isDeprecated, coordSys, (EpsgDatumGeodetic)datum, baseCrs, baseOpCode, kind);

            if (kind == EpsgCrsKind.Geocentric)
                return new EpsgCrsGeocentric(epsgCode, name, area, isDeprecated, coordSys, (EpsgDatumGeodetic)datum, baseCrs, baseOpCode);

            if (kind == EpsgCrsKind.Vertical)
                return new EpsgCrsVertical(epsgCode, name, area, isDeprecated, coordSys, (EpsgDatumVertical)datum);

            if (kind == EpsgCrsKind.Engineering)
                return new EpsgCrsEngineering(epsgCode, name, area, isDeprecated, coordSys, (EpsgDatumEngineering)datum);

            throw new NotImplementedException();
        }

        public EpsgCrs GetByKey(uint targetKey) {
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(DataFileName)) {
                if (_count == 0) {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    _count = reader.ReadUInt16();
                }
                
                return GetByBinarySearch(targetKey, reader);
            }
        }

        private EpsgCrs GetByBinarySearch(uint targetKey, BinaryReader reader) {
            Contract.Requires(_count != 0);
            var baseStream = reader.BaseStream;
            var searchIndexLow = 0;
            var searchIndexHigh = _count - 1;
            while (searchIndexHigh >= searchIndexLow) {
                var searchAtIndex = (searchIndexLow + searchIndexHigh) / 2;
                baseStream.Seek(FileHeaderSize + (searchAtIndex * RecordTotalSize), SeekOrigin.Begin); // seek to the key
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

                if (_count == 0) {
                    baseStream.Seek(0, SeekOrigin.Begin);
                    _count = reader.ReadUInt16();
                }
                else {
                    baseStream.Seek(sizeof(ushort), SeekOrigin.Begin);
                }

                for(ushort i = 0; i < _count; i++) {
                    var key = reader.ReadUInt32();
                    var value = ReadValue(key, reader);
                    yield return value;
                }
            }
        }

    }

    internal sealed class EpsgDataResourceReaderCrsCompound : EpsgDataResourceReaderBasic<EpsgCrsCompound>
    {

        public EpsgDataResourceReaderCrsCompound()
            : base("crscmp.dat", "crs.txt", (sizeof(ushort) * 4) + sizeof(byte))
        { }

        protected override EpsgCrsCompound ReadValue(ushort key, BinaryReader reader) {
            var horizontal = (EpsgCrsDatumBased)EpsgCrs.Get(reader.ReadUInt16());
            var vertical = (EpsgCrsVertical)EpsgCrs.Get(reader.ReadUInt16());
            var area = EpsgArea.Get(reader.ReadUInt16());
            var name = TextReader.GetString(reader.ReadUInt16());
            var isDeprecated = reader.ReadByte() != 0;
            return new EpsgCrsCompound(unchecked((int)key), name, area, isDeprecated, horizontal, vertical);
        }
    }

    internal sealed class EpsgDataResourceReaderParameterValues
    {

        private readonly ushort _operationMethodCode;
        private readonly string _dataFileName;
        private readonly EpsgDataResourceReaderNumbers _numberReader;
        private readonly EpsgDataResourceReaderText _textReader;
        private EpsgParameterUsage[] _usagesCache;

        public EpsgDataResourceReaderParameterValues(ushort operationMethodCode) {
            _operationMethodCode = operationMethodCode;
            _dataFileName = "param" + _operationMethodCode.ToString(CultureInfo.InvariantCulture) + ".dat";
            _textReader = new EpsgDataResourceReaderText("params.txt"); // TODO: reuse one instance
            _numberReader = EpsgDataResourceReaderNumbers.Default;
            _usagesCache = null;
        }

        private EpsgParameterUsage[] ReadParameterUsages(BinaryReader reader) {
            Contract.Ensures(Contract.Result<EpsgParameterUsage[]>() != null);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var usageCount = reader.ReadByte();
            var results = new EpsgParameterUsage[usageCount];
            for (int i = 0; i < usageCount; ++i) {
                var parameterInfo = EpsgParameterInfo.Get(reader.ReadUInt16());
                var signReversal = reader.ReadByte() == 0x01;
                results[i] = new EpsgParameterUsage(parameterInfo, signReversal);
            }
            return results;
        }


        public EpsgParameterUsage[] GetParameterUsages() {
            Contract.Ensures(Contract.Result<EpsgParameterUsage[]>() != null);
            if (_usagesCache == null) {
                using (var reader = EpsgDataResourceReader.CreateBinaryReader(_dataFileName)) {
                    _usagesCache = ReadParameterUsages(reader);
                }
            }
            Contract.Assume(_usagesCache != null);
            return (EpsgParameterUsage[])_usagesCache.Clone(); // always return a new array to prevent mutation
        }

        public List<INamedParameter> ReadParameters(ushort coordinateOperationCode) {
            using (var reader = EpsgDataResourceReader.CreateBinaryReader(_dataFileName)) {
                var baseStream = reader.BaseStream;

                if (_usagesCache == null) {
                    _usagesCache = ReadParameterUsages(reader);
                }
                else {
                    baseStream.Seek(sizeof(byte) + (_usagesCache.Length * (sizeof(byte) + sizeof(ushort))), SeekOrigin.Begin);
                }
                
                Contract.Assume(_usagesCache != null);
                var operationCount = reader.ReadUInt16();
                var operationDataOffset = baseStream.Position;
                const int operationKeySize = sizeof(ushort);
                var operationDataSize = ((sizeof(ushort) * 2) * _usagesCache.Length);
                var operaionRecordSize = operationKeySize + operationDataSize;
                Contract.Assume(operationDataOffset == sizeof(byte) + ((sizeof(ushort) + sizeof(byte)) * _usagesCache.Length) + sizeof(ushort));

                var searchIndexLow = 0;
                var searchIndexHigh = operationCount - 1;
                while (searchIndexHigh >= searchIndexLow) {
                    var searchAtIndex = (searchIndexLow + searchIndexHigh) / 2;
                    baseStream.Seek(operationDataOffset + (searchAtIndex * operaionRecordSize), SeekOrigin.Begin); // seek to the key
                    var localKey = reader.ReadUInt16();
                    if (localKey == coordinateOperationCode)
                        return ReadParametersForOperation(localKey, _usagesCache, reader);
                    else if (localKey < coordinateOperationCode)
                        searchIndexLow = searchAtIndex + 1;
                    else
                        searchIndexHigh = searchAtIndex - 1;
                }
            }
            return null;
        }

        private List<INamedParameter> ReadParametersForOperation(ushort key, EpsgParameterUsage[] usages, BinaryReader reader) {
            Contract.Requires(usages != null);
            Contract.Requires(reader != null);
            var parameters = new List<INamedParameter>(usages.Length);
            foreach(var usage in usages){
                var valueCode = reader.ReadUInt16();
                var uomCode = reader.ReadUInt16();

                EpsgUnit unit;
                if (uomCode == UInt16.MaxValue) {
                    unit = null;
                }
                else {
                    unit = EpsgUnit.Get(uomCode);
                    Contract.Assume(unit != null);
                }

                var parameterName = usage.ParameterInfo.Name;
                if (valueCode != 0xffff) {
                    INamedParameter parameter;
                    if ((valueCode & 0xc000) == 0x8000) {
                        var textValue = _textReader.GetString((ushort)(valueCode & 0x7fff));
                        parameter = new NamedParameter<string>(parameterName, textValue, unit);
                    }
                    else {
                        parameter = new NamedParameter<double>(parameterName, _numberReader.GetValue(valueCode), unit);
                    }
                    parameters.Add(parameter);
                }

            }
            return parameters;
        }

    }

    internal class EpsgDataResourceReaderCoordinateConversionInfo : EpsgDataResourceReaderBasic<EpsgCoordinateOperationInfo> {

        public EpsgDataResourceReaderCoordinateConversionInfo()
            : base("opconv.dat", "op.txt", (sizeof(ushort) * 3) + sizeof(byte)) { }

        protected override EpsgCoordinateOperationInfo ReadValue(ushort key, BinaryReader reader) {
            var opMethodCode = reader.ReadUInt16();
            var areaCode = reader.ReadUInt16();
            var deprecated = reader.ReadByte() != 0;
            var name = TextReader.GetString(reader.ReadUInt16());
            Contract.Assume(!String.IsNullOrEmpty(name));
            return new EpsgCoordinateOperationInfo(key, opMethodCode, areaCode, deprecated, name);
        }

    }

    internal class EpsgDataResourceReaderCoordinateTransformInfo : EpsgDataResourceReaderBasic<EpsgCoordinateTransformInfo> {

        private readonly EpsgDataResourceReaderNumbers _numberReader;


        public EpsgDataResourceReaderCoordinateTransformInfo()
            : base("optran.dat", "op.txt", (sizeof(ushort) * 6) + sizeof(byte))
        {
            _numberReader = EpsgDataResourceReaderNumbers.Default;
        }

        protected override EpsgCoordinateTransformInfo ReadValue(ushort key, BinaryReader reader) {
            var sourceCrsCode = reader.ReadUInt16();
            var targetCrsCode = reader.ReadUInt16();
            var opMethodCode = reader.ReadUInt16();
            var accuracy = _numberReader.GetValue(reader.ReadUInt16());
            var areaCode = reader.ReadUInt16();
            var deprecated = reader.ReadByte() != 0;
            var name = TextReader.GetString(reader.ReadUInt16());
            Contract.Assume(!String.IsNullOrEmpty(name));
            return new EpsgCoordinateTransformInfo(
                key, sourceCrsCode, targetCrsCode, opMethodCode,
                accuracy, areaCode, deprecated, name);
        }

    }

    internal class EpsgDataResourceReaderConcatenatedCoordinateOperationInfo : EpsgDataResourceReaderBasic<EpsgConcatenatedCoordinateOperationInfo> {

        private const string CatPathFileName = "oppath.dat";

        public EpsgDataResourceReaderConcatenatedCoordinateOperationInfo()
            : base("opcat.dat", "op.txt", (sizeof(ushort) * 5) + (sizeof(byte) * 2))
        {

        }

        protected override EpsgConcatenatedCoordinateOperationInfo ReadValue(ushort key, BinaryReader reader) {
            var sourceCrsCode = reader.ReadUInt16();
            var targetCrsCode = reader.ReadUInt16();
            var areaCode = reader.ReadUInt16();
            var deprecated = reader.ReadByte() != 0;
            var name = TextReader.GetString(reader.ReadUInt16());
            Contract.Assume(!String.IsNullOrEmpty(name));
            var stepCodes = new ushort[reader.ReadByte()];
            var stepFileOffset = reader.ReadUInt16();
            using (var readerPath = EpsgDataResourceReader.CreateBinaryReader(CatPathFileName)) {
                readerPath.BaseStream.Seek(stepFileOffset, SeekOrigin.Begin);
                for (int i = 0; i < stepCodes.Length; i++) {
                    stepCodes[i] = readerPath.ReadUInt16();
                }
            }
            return new EpsgConcatenatedCoordinateOperationInfo(
                key, sourceCrsCode, targetCrsCode, areaCode,
                deprecated, name, stepCodes
            );
        }

    }

}
