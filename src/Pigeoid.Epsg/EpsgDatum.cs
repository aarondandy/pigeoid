using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;
using Pigeoid.Transformation;

namespace Pigeoid.Epsg
{

    public static class EpsgDatumRepository
    {

        private const int CodeSize = sizeof(ushort);
        private const string TxtFileName = "datums.txt";

        private static SortedDictionary<ushort, T> GenerateSimpleLookUp<T>(
            string fileName, Func<ushort, string, EpsgArea, T> generate
        ) {
            Contract.Requires(!String.IsNullOrEmpty(fileName));
            Contract.Requires(generate != null);
            Contract.Ensures(Contract.Result<SortedDictionary<ushort, T>>() != null);
            var lookUp = new SortedDictionary<ushort, T>();
            using (var readerTxt = EpsgDataResource.CreateBinaryReader(TxtFileName))
            using (var readerDat = EpsgDataResource.CreateBinaryReader(fileName)) {
                while (readerDat.BaseStream.Position < readerDat.BaseStream.Length) {
                    var code = readerDat.ReadUInt16();
                    var name = EpsgTextLookUp.GetString(readerDat.ReadUInt16(), readerTxt);
                    var area = EpsgArea.Get(readerDat.ReadUInt16());
                    lookUp.Add(code, generate(code, name, area));
                }
            }
            return lookUp;
        }

        internal class EpsgDatumGeodeticLookUp : EpsgDynamicLookUpBase<ushort, EpsgDatumGeodetic>
        {
            private const string DatFileName = "datumgeo.dat";
            private const int RecordDataSize = sizeof(ushort) * 4;
            private const int RecordSize = sizeof(ushort) + RecordDataSize;

            private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

            private static ushort[] GetAllKeys() {
                Contract.Ensures(Contract.Result<ushort[]>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    var keys = new List<ushort>();
                    while (reader.BaseStream.Position < reader.BaseStream.Length) {
                        keys.Add(reader.ReadUInt16());
                        reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
                    }
                    return keys.ToArray();
                }
            }

            public EpsgDatumGeodeticLookUp() : base(GetAllKeys()) { }

            protected override EpsgDatumGeodetic Create(ushort key, int index) {
                Contract.Ensures(Contract.Result<EpsgDatumGeodetic>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    var area = EpsgArea.Get(reader.ReadUInt16());
                    var spheroid = EpsgEllipsoid.Get(reader.ReadUInt16());
                    var meridian = EpsgPrimeMeridian.Get(reader.ReadUInt16());
                    return new EpsgDatumGeodetic(key, name, spheroid, meridian, area);
                }
            }

            protected override ushort GetKeyForItem(EpsgDatumGeodetic value) {
                Contract.Requires(value != null);
                return (ushort)value.Code;
            }
        }

        private static readonly Lazy<EpsgFixedLookUpBase<ushort, EpsgDatumEngineering>> LookUpEngineeringCore = new Lazy<EpsgFixedLookUpBase<ushort, EpsgDatumEngineering>>(
            () => new EpsgFixedLookUpBase<ushort, EpsgDatumEngineering>(GenerateSimpleLookUp(
                "datumegr.dat",
                (code, name, area) => new EpsgDatumEngineering(code, name, area)
            )),
            LazyThreadSafetyMode.ExecutionAndPublication
        );

        private static readonly Lazy<EpsgFixedLookUpBase<ushort, EpsgDatumVertical>> LookUpVerticalCore = new Lazy<EpsgFixedLookUpBase<ushort, EpsgDatumVertical>>(
            () => new EpsgFixedLookUpBase<ushort, EpsgDatumVertical>(GenerateSimpleLookUp(
                "datumver.dat",
                (code, name, area) => new EpsgDatumVertical(code, name, area)
            )),
            LazyThreadSafetyMode.ExecutionAndPublication
        );

        internal static EpsgFixedLookUpBase<ushort, EpsgDatumEngineering> LookUpEngineering {
            get {
                Contract.Ensures(Contract.Result<EpsgFixedLookUpBase<ushort, EpsgDatumEngineering>>() != null);
                return LookUpEngineeringCore.Value;
            }
        }

        internal static EpsgFixedLookUpBase<ushort, EpsgDatumVertical> LookUpVertical {
            get {
                Contract.Ensures(Contract.Result<EpsgFixedLookUpBase<ushort, EpsgDatumVertical>>() != null);
                return LookUpVerticalCore.Value;
            }
        }

        internal static readonly EpsgDatumGeodeticLookUp LookUpGeodetic = new EpsgDatumGeodeticLookUp();

        public static EpsgDatum Get(int code) {
            return code >= 0 && code < UInt16.MaxValue
                ? RawGet((ushort)code)
                : null;
        }

        private static EpsgDatum RawGet(ushort code) {
            return LookUpGeodetic.Get(code)
                ?? LookUpVertical.Get(code)
                ?? LookUpEngineering.Get(code)
                as EpsgDatum;
        }

        public static EpsgDatumGeodetic GetGeodetic(int code) {
            return code >= 0 && code < UInt16.MaxValue
                ? LookUpGeodetic.Get((ushort)code)
                : null;
        }

        public static IEnumerable<EpsgDatum> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgDatum>>() != null);
                return LookUpGeodetic.Values
                    .Union<EpsgDatum>(LookUpVertical.Values)
                    .Union(LookUpEngineering.Values)
                    .OrderBy(x => x.Code);
            }
        }

    }

    public abstract class EpsgDatum : IDatum
    {

        public static IEnumerable<EpsgDatum> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgDatum>>() != null);
                return EpsgDatumRepository.Values;
            }
        }

        public static EpsgDatum Get(int code) {
            return EpsgDatumRepository.Get(code);
        }

        private readonly ushort _code;

        internal EpsgDatum(ushort code, string name, EpsgArea area) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            _code = code;
            Name = name;
            Area = area;
        }

        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(Area != null);
        }

        public int Code { get { return _code; } }

        public string Name { get; private set; }

        public EpsgArea Area { get; private set; }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

        public abstract string Type { get; }

    }

    public class EpsgDatumEngineering : EpsgDatum
    {
        internal EpsgDatumEngineering(ushort code, string name, EpsgArea area) : base(code, name, area) { }
        public override string Type { get { return "Engineering"; } }
    }

    public class EpsgDatumVertical : EpsgDatum
    {
        internal EpsgDatumVertical(ushort code, string name, EpsgArea area) : base(code, name, area) { }
        public override string Type { get { return "Vertical"; } }
    }

    public class EpsgDatumGeodetic : EpsgDatum, IDatumGeodetic
    {

        internal EpsgDatumGeodetic(ushort code, string name, EpsgEllipsoid spheroid, EpsgPrimeMeridian primeMeridian, EpsgArea area)
            : base(code, name, area) {
            Contract.Requires(spheroid != null);
            Contract.Requires(primeMeridian != null);
            Spheroid = spheroid;
            PrimeMeridian = primeMeridian;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Spheroid != null);
            Contract.Invariant(PrimeMeridian != null);
        }

        public EpsgEllipsoid Spheroid { get; private set; }

        ISpheroidInfo IDatumGeodetic.Spheroid { get { return Spheroid; } }

        public EpsgPrimeMeridian PrimeMeridian { get; private set; }

        IPrimeMeridianInfo IDatumGeodetic.PrimeMeridian { get { return PrimeMeridian; }  }

        public override string Type { get { return "Geodetic"; } }

        public Helmert7Transformation BasicWgs84Transformation {
            get { throw new NotImplementedException(); } // TODO: this
        }

        public bool IsTransformableToWgs84 {
            get { throw new NotImplementedException(); } // TODO: this
        }
    }
}
