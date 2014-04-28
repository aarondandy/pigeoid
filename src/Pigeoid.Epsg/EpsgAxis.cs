using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;
using Pigeoid.Epsg.Utility;

namespace Pigeoid.Epsg
{
    public class EpsgAxis : IAxis
    {

        private class EpsgAxisSet
        {
            public EpsgAxisSet(ushort key, EpsgAxis[] axes) {
                Contract.Requires(axes != null);
                Contract.Requires(Contract.ForAll(axes, x => x != null));
                CsKey = key;
                Axes = axes;
            }

            public ushort CsKey { get; private set; }
            public EpsgAxis[] Axes { get; private set; }

            [ContractInvariantMethod]
            private void ObjectInvariants() {
                Contract.Invariant(Axes != null);
                Contract.Invariant(Contract.ForAll(Axes, x => x != null));
            }

        }

        private class EpsgAxisSetLookUp : EpsgDynamicLookUpBase<ushort, EpsgAxisSet>
        {
            private const string DatFileName = "axis.dat";
            private const int AxisRecordSize = sizeof(ushort) * 4;
            private const int CodeSize = sizeof(ushort);

            private class KeyData
            {
                public ushort[] KeyLookUp;
                public Dictionary<ushort, ushort> KeyAddress;
            }

            private static KeyData GetKeyData() {
                Contract.Ensures(Contract.Result<KeyData>() != null);
                Contract.Ensures(Contract.Result<KeyData>().KeyLookUp != null);
                Contract.Ensures(Contract.Result<KeyData>().KeyAddress != null);
                var keyData = new KeyData();
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    keyData.KeyLookUp = new ushort[reader.ReadUInt16()];
                    keyData.KeyAddress = new Dictionary<ushort, ushort>(keyData.KeyLookUp.Length);
                    for (int i = 0; i < keyData.KeyLookUp.Length; i++) {
                        var address = (ushort)reader.BaseStream.Position;
                        var key = reader.ReadUInt16();
                        keyData.KeyLookUp[i] = key;
                        keyData.KeyAddress[key] = address;
                        var axesCount = reader.ReadByte();
                        reader.BaseStream.Seek(axesCount * AxisRecordSize, SeekOrigin.Current);
                    }
                }
                return keyData;
            }

            private readonly KeyData _keyData;

            public EpsgAxisSetLookUp() : this(GetKeyData()) { }

            private EpsgAxisSetLookUp(KeyData keyData)
                : base(keyData.KeyLookUp) {
                Contract.Requires(keyData != null);
                Contract.Requires(keyData.KeyLookUp != null);
                _keyData = keyData;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_keyData != null);
            }

            protected override EpsgAxisSet Create(ushort key, int index) {
                Contract.Ensures(Contract.Result<EpsgAxisSet>() != null);
                using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
                    reader.BaseStream.Seek(_keyData.KeyAddress[key] + CodeSize, SeekOrigin.Begin);
                    var axes = new EpsgAxis[reader.ReadByte()];
                    for (int i = 0; i < axes.Length; i++) {
                        var uom = EpsgUnit.Get(reader.ReadUInt16());
                        Contract.Assume(uom != null);
                        using (var textReader = EpsgDataResource.CreateBinaryReader("axis.txt")) {
                            var name = EpsgTextLookUp.GetString(reader.ReadUInt16(), textReader);
                            Contract.Assume(!String.IsNullOrEmpty(name));
                            var orientation = EpsgTextLookUp.GetString(reader.ReadUInt16(), textReader);
                            var abbreviation = EpsgTextLookUp.GetString(reader.ReadUInt16(), textReader);
                            axes[i] = new EpsgAxis(name, abbreviation, orientation, uom);
                        }
                    }
                    Contract.Assume(Contract.ForAll(axes, x => x != null));
                    return new EpsgAxisSet(key, axes);
                }
            }

            protected override ushort GetKeyForItem(EpsgAxisSet value) {
                return value.CsKey;
            }

        }

        private static readonly EpsgAxisSetLookUp SetLookUp = new EpsgAxisSetLookUp();

        internal static ReadOnlyCollection<EpsgAxis> Get(ushort csCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgAxis>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<ReadOnlyCollection<EpsgAxis>>(), x => x != null));
            var set = SetLookUp.Get(csCode);
            var axes = set == null ? new EpsgAxis[0] : set.Axes;
            return axes.AsReadOnly();
        }

        private EpsgAxis(string name, string abbreviation, string orientation, EpsgUnit unit) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(!String.IsNullOrEmpty(abbreviation));
            Contract.Requires(!String.IsNullOrEmpty(orientation));
            Contract.Requires(unit != null);
            Name = name;
            Abbreviation = abbreviation;
            Orientation = orientation;
            Unit = unit;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(!String.IsNullOrEmpty(Abbreviation));
            Contract.Invariant(!String.IsNullOrEmpty(Orientation));
            Contract.Invariant(Unit != null);
        }

        public EpsgUnit Unit { get; private set; }

        public string Name { get; private set; }

        public string Abbreviation { get; private set; }

        public string Orientation { get; private set; }

    }
}
