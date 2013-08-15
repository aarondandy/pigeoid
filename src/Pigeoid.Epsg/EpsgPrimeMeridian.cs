using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.Epsg.Resources;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
    public class EpsgPrimeMeridian : IPrimeMeridianInfo
    {

        internal static readonly EpsgFixedLookUpBase<ushort, EpsgPrimeMeridian> LookUp;

        static EpsgPrimeMeridian() {
            var lookUpDictionary = new SortedDictionary<ushort, EpsgPrimeMeridian>();
            using (var readerTxt = EpsgDataResource.CreateBinaryReader("meridians.txt"))
            using (var numberLookUp = new EpsgNumberLookUp())
            using (var readerDat = EpsgDataResource.CreateBinaryReader("meridians.dat")) {
                for (int i = readerDat.ReadUInt16(); i > 0; i--) {
                    var code = readerDat.ReadUInt16();
                    var uom = EpsgUnit.Get(readerDat.ReadUInt16());
                    var longitude = numberLookUp.Get(readerDat.ReadUInt16());
                    var name = EpsgTextLookUp.GetString(readerDat.ReadByte(), readerTxt);
                    lookUpDictionary.Add(code, new EpsgPrimeMeridian(code, name, longitude, uom));
                }
            }
            LookUp = new EpsgFixedLookUpBase<ushort, EpsgPrimeMeridian>(lookUpDictionary);
        }

        public static EpsgPrimeMeridian Get(int code) {
            return code >= 0 && code < ushort.MaxValue ? LookUp.Get(unchecked((ushort)code)) : null;
        }

        public static IEnumerable<EpsgPrimeMeridian> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgPrimeMeridian>>() != null);
                return LookUp.Values;
            }
        }

        private readonly ushort _code;

        private EpsgPrimeMeridian(ushort code, string name, double longitude, EpsgUnit unit) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(unit != null);
            _code = code;
            Unit = unit;
            Longitude = longitude;
            Name = name;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(Unit != null);
        }

        public int Code { get { return _code; } }

        public string Name { get; private set; }

        public double Longitude { get; private set; }

        public EpsgUnit Unit { get; private set; }

        IUnit IPrimeMeridianInfo.Unit { get { return Unit; } }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

    }
}
