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

        [Obsolete]
        internal static readonly EpsgDataResourceReaderEllipsoid Reader = new EpsgDataResourceReaderEllipsoid();

        [Obsolete]
        public static EpsgEllipsoid Get(int code) {
            return code >= 0 && code < ushort.MaxValue
                ? Reader.GetByKey((ushort)code)
                : null;
        }

        [Obsolete]
        public static IEnumerable<EpsgEllipsoid> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgEllipsoid>>() != null);
                return Reader.ReadAllValues();
            }
        }

        private readonly ushort _code;

        internal EpsgEllipsoid(ushort code, string name, EpsgUnit unit, ISpheroid<double> core) {
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
