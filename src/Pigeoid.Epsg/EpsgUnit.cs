using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Pigeoid.Epsg.Resources;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
    public class EpsgUnit : IUnit, IAuthorityBoundEntity
    {

        private readonly ushort _code;
        [Obsolete("Units should not have knowledge of conversion")]
        private readonly EpsgMicroDatabase _generatingDatabase;

        internal EpsgUnit(ushort code, string name, string type, double factorB, double factorC, EpsgMicroDatabase generatingDatabase) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(!String.IsNullOrEmpty(type));
// ReSharper disable CompareOfFloatsByEqualityOperator
            Contract.Requires(factorC != 0);
// ReSharper restore CompareOfFloatsByEqualityOperator
            _code = code;
            Name = name;
            Type = type;
            FactorB = factorB;
            FactorC = factorC;
            _generatingDatabase = generatingDatabase;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(!String.IsNullOrEmpty(Type));
// ReSharper disable CompareOfFloatsByEqualityOperator
            Contract.Invariant(FactorC != 0);
// ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public int Code { get { return _code; } }

        public double FactorB { get; private set; }

        public double FactorC { get; private set; }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

        internal IUnitConversion<double> BuildConversionToBase() {
            var to = GetBaseUnit();
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (Double.IsNaN(FactorC))
                return null;
            if (1.0 == FactorB && 1.0 == FactorC)
                return new UnitUnityConversion(this, to);
            if (1.0 != FactorB && 1.0 == FactorC)
                return new UnitScalarConversion(this, to, FactorB);
            return new UnitRatioConversion(this, to, FactorB, FactorC);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        private IUnit GetBaseUnit() {
            return _generatingDatabase.GetBaseUnit(Type);
        }

        public IUnitConversionMap<double> ConversionMap {
            get {
                Contract.Ensures(Contract.Result<IUnitConversionMap<double>>() != null);
                return _generatingDatabase.UnitConversionMap;
            }
        }

        public override string ToString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return Name;
        }
    }
}
