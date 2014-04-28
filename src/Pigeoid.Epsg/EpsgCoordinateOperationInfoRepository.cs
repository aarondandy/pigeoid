using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Epsg.Resources;
using System.Collections.ObjectModel;

namespace Pigeoid.Epsg
{
    public class EpsgCoordinateOperationInfoRepository
    {

        internal class EpsgCoordinateConversionInfoLookUp : EpsgDynamicLookUpBase<ushort, EpsgCoordinateOperationInfo>
        {
            private const string DatFileName = "opconv.dat";
            private const string TxtFileName = "op.txt";
            private const int RecordDataSize = (sizeof(ushort) * 3) + sizeof(byte);
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

            public EpsgCoordinateConversionInfoLookUp() : base(GetKeys()) { }

            protected override EpsgCoordinateOperationInfo Create(ushort code, int index) {
                Contract.Ensures(Contract.Result<EpsgCoordinateOperationInfo>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
                    var opMethodCode = reader.ReadUInt16();
                    var areaCode = reader.ReadUInt16();
                    var deprecated = reader.ReadByte() != 0;
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    Contract.Assume(!String.IsNullOrEmpty(name));
                    return new EpsgCoordinateOperationInfo(code, opMethodCode, areaCode, deprecated, name);
                }
            }

            protected override ushort GetKeyForItem(EpsgCoordinateOperationInfo value) {
                return (ushort)value.Code;
            }

        }

        internal class EpsgCoordinateTransformInfoLookUp : EpsgDynamicLookUpBase<ushort, EpsgCoordinateTransformInfo>
        {

            private const string DatFileName = "optran.dat";
            private const string TxtFileName = "op.txt";
            private const int RecordDataSize = (sizeof(ushort) * 6) + sizeof(byte);
            private const int RecordDataIndexSkipSize = RecordDataSize - (2 * sizeof(ushort));
            private const int CodeSize = sizeof(ushort);
            private const int RecordSize = CodeSize + RecordDataSize;

            private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

            public static EpsgCoordinateTransformInfoLookUp Create() {
                Contract.Ensures(Contract.Result<EpsgCoordinateTransformInfoLookUp>() != null);
                var forwardLookUp = new Dictionary<ushort, List<ushort>>();
                var reverseLookUp = new Dictionary<ushort, List<ushort>>();
                var keys = new List<ushort>();
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    while (reader.BaseStream.Position < reader.BaseStream.Length) {
                        var key = reader.ReadUInt16();
                        var sourceCrsCode = reader.ReadUInt16();
                        var targetCrsCode = reader.ReadUInt16();
                        reader.BaseStream.Seek(RecordDataIndexSkipSize, SeekOrigin.Current);

                        keys.Add(key);

                        List<ushort> lookUpTargets;
                        if (!forwardLookUp.TryGetValue(sourceCrsCode, out lookUpTargets)) {
                            lookUpTargets = new List<ushort>();
                            forwardLookUp.Add(sourceCrsCode, lookUpTargets);
                        }
                        Contract.Assume(lookUpTargets != null);
                        lookUpTargets.Add(key);

                        if (!reverseLookUp.TryGetValue(targetCrsCode, out lookUpTargets)) {
                            lookUpTargets = new List<ushort>();
                            reverseLookUp.Add(targetCrsCode, lookUpTargets);
                        }
                        Contract.Assume(lookUpTargets != null);
                        lookUpTargets.Add(key);
                    }
                }
                return new EpsgCoordinateTransformInfoLookUp(
                    keys.ToArray(),
                    forwardLookUp.ToDictionary(x => x.Key, x => x.Value.ToArray()),
                    reverseLookUp.ToDictionary(x => x.Key, x => x.Value.ToArray())
                );
            }

            private readonly Dictionary<ushort, ushort[]> _forwardLookUp;
            private readonly Dictionary<ushort, ushort[]> _reverseLookUp;

            private EpsgCoordinateTransformInfoLookUp(
                ushort[] keys,
                Dictionary<ushort, ushort[]> forwardLookUp,
                Dictionary<ushort, ushort[]> reverseLookUp
            )
                : base(keys) {
                Contract.Requires(keys != null);
                Contract.Requires(forwardLookUp != null);
                Contract.Requires(reverseLookUp != null);
                _forwardLookUp = forwardLookUp;
                _reverseLookUp = reverseLookUp;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_forwardLookUp != null);
                Contract.Invariant(_reverseLookUp != null);
            }

            protected override EpsgCoordinateTransformInfo Create(ushort code, int index) {
                Contract.Ensures(Contract.Result<EpsgCoordinateTransformInfo>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName))
                using (var numberLookUp = new EpsgNumberLookUp()) {
                    reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
                    var sourceCrsCode = reader.ReadUInt16();
                    var targetCrsCode = reader.ReadUInt16();
                    var opMethodCode = reader.ReadUInt16();
                    var accuracy = numberLookUp.Get(reader.ReadUInt16());
                    var areaCode = reader.ReadUInt16();
                    var deprecated = reader.ReadByte() != 0;
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    Contract.Assume(!String.IsNullOrEmpty(name));
                    return new EpsgCoordinateTransformInfo(
                        code, sourceCrsCode, targetCrsCode, opMethodCode,
                        accuracy, areaCode, deprecated, name);
                }
            }

            internal ushort[] GetForwardReferencedOperationCodes(int sourceCode) {
                ushort[] data;
                return sourceCode >= 0
                    && sourceCode <= ushort.MaxValue
                    && _forwardLookUp.TryGetValue((ushort)sourceCode, out data)
                    ? data
                    : null;
            }

            internal ushort[] GetReverseReferencedOperationCodes(int targetCode) {
                ushort[] data;
                return targetCode >= 0
                    && targetCode <= ushort.MaxValue
                    && _reverseLookUp.TryGetValue((ushort)targetCode, out data)
                    ? data
                    : null;
            }

            protected override ushort GetKeyForItem(EpsgCoordinateTransformInfo value) {
                return (ushort)value.Code;
            }

        }

        internal class EpsgCoordinateOperationConcatenatedInfoLookUp : EpsgDynamicLookUpBase<ushort, EpsgConcatenatedCoordinateOperationInfo>
        {
            private const string DatFileName = "opcat.dat";
            private const string PathFileName = "oppath.dat";
            private const string TxtFileName = "op.txt";
            private const int RecordDataSize = (sizeof(ushort) * 5) + (sizeof(byte) * 2);
            private const int RecordDataIndexSkipSize = RecordDataSize - (sizeof(ushort) * 2);
            private const int CodeSize = sizeof(ushort);
            private const int RecordSize = CodeSize + RecordDataSize;

            private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

            internal static EpsgCoordinateOperationConcatenatedInfoLookUp Create() {
                Contract.Ensures(Contract.Result<EpsgCoordinateOperationConcatenatedInfoLookUp>() != null);
                var forwardLookUp = new Dictionary<ushort, List<ushort>>();
                var reverseLookUp = new Dictionary<ushort, List<ushort>>();
                var keys = new List<ushort>();
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    while (reader.BaseStream.Position < reader.BaseStream.Length) {
                        var key = reader.ReadUInt16();
                        var sourceCrsCode = reader.ReadUInt16();
                        var targetCrsCode = reader.ReadUInt16();
                        reader.BaseStream.Seek(RecordDataIndexSkipSize, SeekOrigin.Current);

                        keys.Add(key);

                        List<ushort> lookUpTargets;
                        if (!forwardLookUp.TryGetValue(sourceCrsCode, out lookUpTargets)) {
                            lookUpTargets = new List<ushort>();
                            forwardLookUp.Add(sourceCrsCode, lookUpTargets);
                        }
                        Contract.Assume(lookUpTargets != null);
                        lookUpTargets.Add(key);

                        if (!reverseLookUp.TryGetValue(targetCrsCode, out lookUpTargets)) {
                            lookUpTargets = new List<ushort>();
                            reverseLookUp.Add(targetCrsCode, lookUpTargets);
                        }
                        Contract.Assume(lookUpTargets != null);
                        lookUpTargets.Add(key);
                    }
                }
                return new EpsgCoordinateOperationConcatenatedInfoLookUp(
                    keys.ToArray(),
                    forwardLookUp.ToDictionary(x => x.Key, x => x.Value.ToArray()),
                    reverseLookUp.ToDictionary(x => x.Key, x => x.Value.ToArray())
                );
            }

            private readonly Dictionary<ushort, ushort[]> _forwardLookUp;
            private readonly Dictionary<ushort, ushort[]> _reverseLookUp;

            private EpsgCoordinateOperationConcatenatedInfoLookUp(
                ushort[] keys,
                Dictionary<ushort, ushort[]> forwardLookUp,
                Dictionary<ushort, ushort[]> reverseLookUp
            ) : base(keys) {
                Contract.Requires(keys != null);
                Contract.Requires(forwardLookUp != null);
                Contract.Requires(reverseLookUp != null);
                _forwardLookUp = forwardLookUp;
                _reverseLookUp = reverseLookUp;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_forwardLookUp != null);
                Contract.Invariant(_reverseLookUp != null);
            }

            protected override EpsgConcatenatedCoordinateOperationInfo Create(ushort code, int index) {
                Contract.Ensures(Contract.Result<EpsgConcatenatedCoordinateOperationInfo>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
                    var sourceCrsCode = reader.ReadUInt16();
                    var targetCrsCode = reader.ReadUInt16();
                    var areaCode = reader.ReadUInt16();
                    var deprecated = reader.ReadByte() != 0;
                    var name = TextLookUp.GetString(reader.ReadUInt16());
                    Contract.Assume(!String.IsNullOrEmpty(name));
                    var stepCodes = new ushort[reader.ReadByte()];
                    var stepFileOffset = reader.ReadUInt16();
                    using (var readerPath = EpsgDataResource.CreateBinaryReader(PathFileName)) {
                        readerPath.BaseStream.Seek(stepFileOffset, SeekOrigin.Begin);
                        for (int i = 0; i < stepCodes.Length; i++) {
                            stepCodes[i] = readerPath.ReadUInt16();
                        }
                    }
                    return new EpsgConcatenatedCoordinateOperationInfo(
                        code, sourceCrsCode, targetCrsCode, areaCode,
                        deprecated, name, stepCodes
                    );
                }
            }

            internal ushort[] GetForwardReferencedOperationCodes(int sourceCode) {
                ushort[] data;
                return sourceCode >= 0
                    && sourceCode <= ushort.MaxValue
                    && _forwardLookUp.TryGetValue((ushort)sourceCode, out data)
                    ? data
                    : null;
            }

            internal ushort[] GetReverseReferencedOperationCodes(int targetCode) {
                ushort[] data;
                return targetCode >= 0
                    && targetCode <= ushort.MaxValue
                    && _reverseLookUp.TryGetValue((ushort)targetCode, out data)
                    ? data
                    : null;
            }

            protected override ushort GetKeyForItem(EpsgConcatenatedCoordinateOperationInfo value) {
                return (ushort)value.Code;
            }

        }

        internal static readonly EpsgCoordinateTransformInfoLookUp TransformLookUp = EpsgCoordinateTransformInfoLookUp.Create();
        internal static readonly EpsgCoordinateConversionInfoLookUp ConversionLookUp = new EpsgCoordinateConversionInfoLookUp();
        internal static readonly EpsgCoordinateOperationConcatenatedInfoLookUp ConcatenatedLookUp = EpsgCoordinateOperationConcatenatedInfoLookUp.Create();

        private static readonly ReadOnlyCollection<EpsgCoordinateTransformInfo> EmptyEcti =
            Array.AsReadOnly(new EpsgCoordinateTransformInfo[0]);

        private static readonly ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo> EmptyEccoi =
            Array.AsReadOnly(new EpsgConcatenatedCoordinateOperationInfo[0]);

        public static EpsgCoordinateTransformInfo GetTransformInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? TransformLookUp.Get((ushort)code) : null;
        }

        public static EpsgCoordinateOperationInfo GetConversionInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? ConversionLookUp.Get((ushort)code) : null;
        }

        public static EpsgConcatenatedCoordinateOperationInfo GetConcatenatedInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? ConcatenatedLookUp.Get((ushort)code) : null;
        }

        /// <summary>
        /// Finds either a transformation or conversion for the given code.
        /// </summary>
        /// <param name="code">The code to find.</param>
        /// <returns>The operation for the code.</returns>
        internal static EpsgCoordinateOperationInfo GetOperationInfo(int code) {
            if (code < 0 || code >= UInt16.MaxValue)
                return null;
            var codeShort = (ushort)code;
            return TransformLookUp.Get(codeShort)
                ?? ConversionLookUp.Get(codeShort);
        }

        public static IEnumerable<EpsgCoordinateTransformInfo> TransformInfos {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateTransformInfo>>() != null);
                return TransformLookUp.Values;
            }
        }

        public static IEnumerable<EpsgCoordinateOperationInfo> ConversionInfos {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateOperationInfo>>() != null);
                return ConversionLookUp.Values;
            }
        }

        public static IEnumerable<EpsgConcatenatedCoordinateOperationInfo> ConcatenatedInfos {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgConcatenatedCoordinateOperationInfo>>() != null);
                return ConcatenatedLookUp.Values;
            }
        }

        public static ReadOnlyCollection<EpsgCoordinateTransformInfo> GetTransformForwardReferenced(int sourceCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgCoordinateTransformInfo>>() != null);
            return ConvertToOperations(TransformLookUp.GetForwardReferencedOperationCodes(sourceCode));
        }

        public static ReadOnlyCollection<EpsgCoordinateTransformInfo> GetTransformReverseReferenced(int targetCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgCoordinateTransformInfo>>() != null);
            return ConvertToOperations(TransformLookUp.GetReverseReferencedOperationCodes(targetCode));
        }

        private static ReadOnlyCollection<EpsgCoordinateTransformInfo> ConvertToOperations(ushort[] ids) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgCoordinateTransformInfo>>() != null);
            return null == ids ? EmptyEcti : Array.AsReadOnly(Array.ConvertAll(ids, TransformLookUp.Get));
        }

        public static ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo> GetConcatenatedForwardReferenced(int sourceCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo>>() != null);
            return ConvertToCatOperations(ConcatenatedLookUp.GetForwardReferencedOperationCodes(sourceCode));
        }

        public static ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo> GetConcatenatedReverseReferenced(int targetCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo>>() != null);
            return ConvertToCatOperations(ConcatenatedLookUp.GetReverseReferencedOperationCodes(targetCode));
        }

        private static ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo> ConvertToCatOperations(ushort[] ids) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo>>() != null);
            return null == ids ? EmptyEccoi : Array.AsReadOnly(Array.ConvertAll(ids, ConcatenatedLookUp.Get));
        }
    }
}
