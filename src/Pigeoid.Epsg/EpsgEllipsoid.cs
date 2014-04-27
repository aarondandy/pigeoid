using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.Epsg.Resources;
using Pigeoid.Unit;
using Vertesaur;

namespace Pigeoid.Epsg
{
    public class EpsgEllipsoid : ISpheroidInfo
    {

        internal static readonly EpsgFixedLookUpBase<ushort, EpsgEllipsoid> LookUp;

        static EpsgEllipsoid() {
            var lookUpDictionary = new SortedDictionary<ushort, EpsgEllipsoid>();
            using (var readerTxt = EpsgDataResource.CreateBinaryReader("ellipsoids.txt"))
            using (var numberLookUp = new EpsgNumberLookUp())
            using (var readerDat = EpsgDataResource.CreateBinaryReader("ellipsoids.dat")) {
                for (int i = readerDat.ReadUInt16(); i > 0; i--) {
                    var code = readerDat.ReadUInt16();
                    var semiMajorAxis = numberLookUp.Get(readerDat.ReadUInt16());
                    var valueB = numberLookUp.Get(readerDat.ReadUInt16());
                    var name = EpsgTextLookUp.GetString(readerDat.ReadUInt16(), readerTxt);
                    Contract.Assume(!String.IsNullOrEmpty(name));
                    var uom = EpsgUnit.Get(readerDat.ReadByte() + 9000);
                    Contract.Assume(uom != null);
                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    lookUpDictionary.Add(code, new EpsgEllipsoid(
                        code, name, uom,
                        (valueB == semiMajorAxis)
                            ? new Sphere(semiMajorAxis)
                        : (valueB < semiMajorAxis / 10.0)
                            ? new SpheroidEquatorialInvF(semiMajorAxis, valueB) as ISpheroid<double>
                        : new SpheroidEquatorialPolar(semiMajorAxis, valueB)
                    ));
                    // ReSharper restore CompareOfFloatsByEqualityOperator
                }
            }
            LookUp = new EpsgFixedLookUpBase<ushort, EpsgEllipsoid>(lookUpDictionary);
        }

        public static EpsgEllipsoid Get(int code) {
            return code >= 0 && code < ushort.MaxValue
                ? LookUp.Get((ushort)code)
                : null;
        }

        public static IEnumerable<EpsgEllipsoid> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgEllipsoid>>() != null);
                return LookUp.Values;
            }
        }

        private readonly ushort _code;

        private EpsgEllipsoid(ushort code, string name, EpsgUnit unit, ISpheroid<double> core) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(unit != null);
            Contract.Requires(core != null);
            _code = code;
            Name = name;
            Core = core;
            AxisUnit = unit;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(AxisUnit != null);
            Contract.Invariant(Core != null);
        }

        public int Code { get { return _code; } }

        public string Name { get; private set; }

        public EpsgUnit AxisUnit { get; private set; }

        IUnit ISpheroidInfo.AxisUnit { get { return AxisUnit; } }

        public ISpheroid<double> Core { get; private set; }

        public double A { get { return Core.A; } }

        public double B { get { return Core.B; } }

        public double E { get { return Core.E; } }

        public double ESecond { get { return Core.ESecond; } }

        public double ESecondSquared { get { return Core.ESecondSquared; } }

        public double ESquared { get { return Core.ESquared; } }

        public double F { get { return Core.F; } }

        public double InvF { get { return Core.InvF; } }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

    }
}
