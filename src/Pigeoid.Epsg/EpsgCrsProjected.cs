using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg.Resources;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
    public class EpsgCrsProjected : EpsgCrsGeodetic, ICrsProjected
    {
        /*
        [Obsolete]
        internal class EpsgCrsProjectedLookUp : EpsgDynamicLookUpBase<int, EpsgCrsProjected>
        {
            private const string DatFileName = "crsprj.dat";
            private const string TxtFileName = "crs.txt";
            private const int RecordDataSize = (sizeof(ushort) * 5) + (sizeof(byte) * 2);
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
                        Contract.Assume(codeList != null);
                        codeList.Add(key);
                    }
                }
                var reverseArrayIndex = reverseIndex.ToDictionary(x => x.Key, x => x.Value.ToArray());
                Contract.Assume(Contract.ForAll(reverseArrayIndex.Values, x => x != null));
                return new EpsgCrsProjectedLookUp(keys.ToArray(), reverseArrayIndex);
            }

            private readonly Dictionary<ushort, int[]> _reverseIndex;

            private EpsgCrsProjectedLookUp(int[] keys, Dictionary<ushort, int[]> reverseIndex)
                : base(keys) {
                Contract.Requires(keys != null);
                Contract.Requires(reverseIndex != null);
                Contract.Requires(Contract.ForAll(reverseIndex.Values, x => x != null));
                _reverseIndex = reverseIndex;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_reverseIndex != null);
                Contract.Invariant(Contract.ForAll(_reverseIndex.Values, x => x != null));
            }

            public ReadOnlyCollection<int> GetProjectionCodesBasedOn(int baseCrsCode) {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<int>>() != null);
                int[] rawList;
                if (baseCrsCode >= 0
                    && baseCrsCode <= ushort.MaxValue
                    && _reverseIndex.TryGetValue((ushort) baseCrsCode, out rawList)
                ) {
                    Contract.Assume(rawList != null);
                    return Array.AsReadOnly(rawList);
                }
                return EmptyIntList;
            }

            protected override EpsgCrsProjected Create(int code, int index) {
                Contract.Ensures(Contract.Result<EpsgCrsProjected>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
                    var baseCrs = EpsgCrs.Get(reader.ReadUInt16());
                    Contract.Assume(baseCrs != null);
                    Contract.Assume(baseCrs is EpsgCrsProjected || baseCrs is EpsgCrsGeodetic);
                    var projectionCode = reader.ReadUInt16();
                    var cs = EpsgCoordinateSystem.Get(reader.ReadUInt16());
                    Contract.Assume(cs!= null);
                    var area = EpsgArea.Get(reader.ReadUInt16());
                    Contract.Assume(area != null);
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    Contract.Assume(!String.IsNullOrEmpty(name));
                    var deprecated = reader.ReadByte() == 0xff;
                    var kind = (EpsgCrsKind)reader.ReadByte();
                    return new EpsgCrsProjected(code, name, area, deprecated, baseCrs, cs, projectionCode, kind);
                }
            }

            protected override int GetKeyForItem(EpsgCrsProjected value) {
                return value.Code;
            }
        }

        [Obsolete]
        internal static readonly EpsgCrsProjectedLookUp LookUp = EpsgCrsProjectedLookUp.Create();
        

        [Obsolete]
        public static EpsgCrsProjected GetProjected(int code) {
            return LookUp.Get(code);
        }

        [Obsolete]
        public static IEnumerable<EpsgCrsProjected> ProjectedValues {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCrsProjected>>() != null);
                return LookUp.Values;
            }
        } */

        /*
        [Obsolete]
        public static ReadOnlyCollection<int> GetProjectionCodesBasedOn(int baseCrsCode) {
            return LookUp.GetProjectionCodesBasedOn(baseCrsCode);
        }

        [Obsolete]
        public static IEnumerable<EpsgCrsProjected> GetProjectionsBasedOn(int baseCrsCode) {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCrsProjected>>() != null);
            throw new NotImplementedException();
            var projectionCodes = GetProjectionCodesBasedOn(baseCrsCode);
            var result = new List<EpsgCrsProjected>(projectionCodes.Count);
            result.AddRange(
                projectionCodes
                    .Select(GetProjected)
                    .Where(projection => null != projection)
            );
            return result;
        }*/

        private readonly int _projectionCode;

        internal EpsgCrsProjected(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic datum, EpsgCrsGeodetic baseCrs, int projectionCode)
            : base(code, name, area, deprecated, cs, datum, baseCrs) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(baseCrs != null);
            Contract.Requires(cs != null);
            Contract.Requires(datum != null);
            _projectionCode = projectionCode;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(BaseCrs != null);
            Contract.Invariant(CoordinateSystem != null);
        }


        ICrsGeodetic ICrsProjected.BaseCrs { get { return BaseCrs; } }

        public EpsgCoordinateOperationInfo Projection {
            get {
                return EpsgCoordinateOperationInfoRepository.GetOperationInfo(_projectionCode);
            }
        }

        ICoordinateOperationInfo ICrsProjected.Projection { get { return Projection; } }

        public override EpsgCrsKind Kind { get { return EpsgCrsKind.Projected; } }
    }
}
