using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
    public class EpsgCrsProjected : EpsgCrs, ICrsProjected
    {

        internal class EpsgCrsProjectedLookUp : EpsgDynamicLookUpBase<int, EpsgCrsProjected>
        {
            private const string DatFileName = "crsprj.dat";
            private const string TxtFileName = "crs.txt";
            private const int RecordDataSize = (sizeof(ushort) * 5) + sizeof(byte);
            private const int CodeSize = sizeof(uint);
            private const int RecordSize = CodeSize + RecordDataSize;
            private const int RecordIndexSkipSize = RecordDataSize - sizeof(ushort);

            private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);
            private static readonly ReadOnlyCollection<int> EmptyIntList = Array.AsReadOnly(new int[0]);

            public static EpsgCrsProjectedLookUp Create() {
                Contract.Ensures(Contract.Result<EpsgCrsProjectedLookUp>() != null);
                var keys = new List<int>();
                var reverseIndex = new Dictionary<ushort, List<int>>();
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    while (reader.BaseStream.Position < reader.BaseStream.Length) {
                        var key = (int)reader.ReadUInt32();
                        var baseCrs = reader.ReadUInt16();
                        reader.BaseStream.Seek(RecordIndexSkipSize, SeekOrigin.Current);

                        keys.Add(key);
                        List<int> codeList;
                        if (!reverseIndex.TryGetValue(baseCrs, out codeList)) {
                            codeList = new List<int>();
                            reverseIndex.Add(baseCrs, codeList);
                        }
                        codeList.Add(key);
                    }
                }
                return new EpsgCrsProjectedLookUp(keys.ToArray(), reverseIndex.ToDictionary(x => x.Key, x => x.Value.ToArray()));
            }

            private readonly Dictionary<ushort, int[]> _reverseIndex;

            private EpsgCrsProjectedLookUp(int[] keys, Dictionary<ushort, int[]> reverseIndex)
                : base(keys) {
                Contract.Requires(keys != null);
                Contract.Requires(reverseIndex != null);
                _reverseIndex = reverseIndex;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_reverseIndex != null);
            }

            public ReadOnlyCollection<int> GetProjectionCodesBasedOn(int baseCrsCode) {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<int>>() != null);
                int[] rawList;
                return baseCrsCode >= 0
                    && baseCrsCode <= ushort.MaxValue
                    && _reverseIndex.TryGetValue((ushort)baseCrsCode, out rawList)
                    ? Array.AsReadOnly(rawList)
                    : EmptyIntList;
            }

            protected override EpsgCrsProjected Create(int code, int index) {
                Contract.Ensures(Contract.Result<EpsgCrsProjected>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
                    var baseCrs = EpsgCrs.Get(reader.ReadUInt16());
                    var projectionCode = reader.ReadUInt16();
                    var cs = EpsgCoordinateSystem.Get(reader.ReadUInt16());
                    var area = EpsgArea.Get(reader.ReadUInt16());
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    var deprecated = reader.ReadByte() == 0xff;
                    return new EpsgCrsProjected(code, name, area, deprecated, baseCrs, cs, projectionCode);
                }
            }

            protected override int GetKeyForItem(EpsgCrsProjected value) {
                Contract.Requires(value != null);
                return value.Code;
            }
        }

        internal static readonly EpsgCrsProjectedLookUp LookUp = EpsgCrsProjectedLookUp.Create();

        public static EpsgCrsProjected GetProjected(int code) {
            return LookUp.Get(code);
        }

        public static IEnumerable<EpsgCrsProjected> ProjectedValues {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCrsProjected>>() != null);
                return LookUp.Values;
            }
        }

        public static ReadOnlyCollection<int> GetProjectionCodesBasedOn(int baseCrsCode) {
            return LookUp.GetProjectionCodesBasedOn(baseCrsCode);
        }

        public static IEnumerable<EpsgCrsProjected> GetProjectionsBasedOn(int baseCrsCode) {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCrsProjected>>() != null);
            var projectionCodes = GetProjectionCodesBasedOn(baseCrsCode);
            var result = new List<EpsgCrsProjected>(projectionCodes.Count);
            result.AddRange(
                projectionCodes
                    .Select(GetProjected)
                    .Where(projection => null != projection)
            );
            return result;
        }

        private static EpsgCrsGeodetic FindGeodeticBase(EpsgCrs crs) {
            do {
                if (crs is EpsgCrsGeodetic)
                    return crs as EpsgCrsGeodetic;

                if (crs is EpsgCrsProjected)
                    crs = (crs as EpsgCrsProjected).BaseCrs;

            } while (null != crs);
            return null;
        }

        private readonly int _projectionCode;

        internal EpsgCrsProjected(int code, string name, EpsgArea area, bool deprecated, EpsgCrs baseCrs, EpsgCoordinateSystem cs, int projectionCode)
            : base(code, name, area, deprecated) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(baseCrs != null);
            Contract.Requires(cs != null);
            BaseCrs = baseCrs;
            CoordinateSystem = cs;
            _projectionCode = projectionCode;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(BaseCrs != null);
            Contract.Invariant(CoordinateSystem != null);
        }

        public EpsgCoordinateSystem CoordinateSystem { get; private set; }

        public EpsgCrs BaseCrs { get; private set; }

        public EpsgCrsGeodetic BaseGeodeticCrs { get { return FindGeodeticBase(BaseCrs); } }

        ICrsGeodetic ICrsProjected.BaseCrs { get { return BaseGeodeticCrs; } }

        public EpsgCoordinateOperationInfo Projection {
            get {
                return EpsgCoordinateOperationInfoRepository.GetOperationInfo(_projectionCode);
            }
        }

        ICoordinateOperationInfo ICrsProjected.Projection { get { return Projection; } }

        public EpsgDatumGeodetic Datum {
            get {
                Contract.Ensures(Contract.Result<EpsgDatumGeodetic>() != null);
                return BaseGeodeticCrs.GeodeticDatum;
            }
        }

        IDatumGeodetic ICrsGeodetic.Datum { get { return Datum; } }

        public EpsgUnit Unit {
            get {
                Contract.Ensures(Contract.Result<EpsgUnit>() != null);
                return CoordinateSystem.Axes.First().Unit;
            }
        }

        IUnit ICrsGeodetic.Unit { get { return Unit; } }

        public IList<EpsgAxis> Axes {
            get {
                Contract.Ensures(Contract.Result<IList<EpsgAxis>>() != null);
                return CoordinateSystem.Axes.ToArray();
            }
        }

        IList<IAxis> ICrsGeodetic.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }
    }
}
