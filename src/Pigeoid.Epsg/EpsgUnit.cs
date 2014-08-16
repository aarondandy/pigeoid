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

        [Obsolete]
        private static readonly EpsgDataResourceAllUnitsReader Readers = new EpsgDataResourceAllUnitsReader();

        [Obsolete]
        private static readonly ReadOnlyUnitConversionMap AllConversionMap;

        [Obsolete]
        static EpsgUnit() {

            var indianFoot = Readers.GetByKey(9080);
            Contract.Assume(indianFoot != null);
            var britishFoot = Readers.GetByKey(9070);
            Contract.Assume(britishFoot != null);

            var degree9102 = Readers.GetByKey(9102);
            Contract.Assume(degree9102 != null);
            var degree9122 = Readers.GetByKey(9122);
            Contract.Assume(degree9122 != null);
            var arcMinute = Readers.GetByKey(9103);
            Contract.Assume(arcMinute != null);
            var arcSecond = Readers.GetByKey(9103);
            Contract.Assume(arcSecond != null);
            var grad = Readers.GetByKey(9105);
            Contract.Assume(grad != null);
            var gon = Readers.GetByKey(9106);
            Contract.Assume(gon != null);
            var centesimalMinute = Readers.GetByKey(9112);
            Contract.Assume(centesimalMinute != null);
            var centesimalSecond = Readers.GetByKey(9113);
            Contract.Assume(centesimalSecond != null);
            var dms = Readers.GetByKey(9110);
            Contract.Assume(dms != null);
            var dm = Readers.GetByKey(9111);
            Contract.Assume(dm != null);

            AllConversionMap = new ReadOnlyUnitConversionMap(
                Readers.ReadAllValues()
                    .Select(x => x.BuildConversionToBase())
                    .Where(x => null != x)
                    .Concat(new IUnitConversion<double>[] {
                        CreateScalarConversion(9014, 9002, 6), // fathom to foot
                        CreateScalarConversion(9093, 9002, 5280), // mile to foot
                        CreateScalarConversion(9096, 9002, 3), // yard to foot
                        CreateScalarConversion(9097, 9096, 22), // chain to yard
                        CreateScalarConversion(9097, 9098, 100), // chain to link

                        CreateScalarConversion(9037, 9005, 3),    // clarke yard to clarke foot
                        CreateScalarConversion(9038, 9037, 22),   // clarke chain to clarke yard
                        CreateScalarConversion(9038, 9039, 100),  // clarke chain to clarke link

                        CreateScalarConversion(9040, 9041, 3),    // British yard to foot (Sears 1922)
                        CreateScalarConversion(9042, 9040, 22),   // British chain to yard (Sears 1922)
                        CreateScalarConversion(9042, 9043, 100),  // British chain to link (Sears 1922)

                        CreateScalarConversion(9050, 9051, 3),    // British yard to foot (Benoit 1895 A)
                        CreateScalarConversion(9052, 9050, 22),   // British chain to yard (Benoit 1895 A)
                        CreateScalarConversion(9052, 9053, 100),  // British chain to link (Benoit 1895 A)

                        CreateScalarConversion(9060, 9061, 3),    // British yard to foot (Benoit 1895 B)
                        CreateScalarConversion(9062, 9060, 22),   // British chain to yard (Benoit 1895 B)
                        CreateScalarConversion(9062, 9063, 100),  // British chain to link (Benoit 1895 B)

                        CreateScalarConversion(9084, 9080, 3),    // Indian yard to foot
                        CreateScalarConversion(9085, 9081, 3),    // Indian yard to foot (1937)
                        CreateScalarConversion(9086, 9082, 3),    // Indian yard to foot (1962)
                        CreateScalarConversion(9087, 9083, 3),    // Indian yard to foot (1975)

                        CreateScalarConversion(9099, 9300, 3),    // British yard to foot (Sears 1922 truncated)
                        CreateScalarConversion(9301, 9099, 22),   // British chain to yard (Sears 1922 truncated)
                        CreateScalarConversion(9301, 9302, 100),  // British chain to link (Sears 1922 truncated)

                        new UnitRatioConversion(indianFoot, britishFoot, 49999783, 50000000), // indian foot to british foot 1865

                        new UnitUnityConversion(degree9102, degree9122), 

                        new UnitScalarConversion(degree9102, arcMinute, 60), // degree to arc-minute
                        new UnitScalarConversion(arcMinute, arcSecond, 60), // arc-minute to arc-second

                        new UnitUnityConversion(grad, gon), // grad and gon are the same
                        new UnitRatioConversion(grad, degree9102, 9, 10), // grad to degree
                        new UnitRatioConversion(gon, degree9102, 9, 10), // gon to degree
                        new UnitScalarConversion(grad, centesimalMinute, 100), // grad to centesimal minute
                        new UnitScalarConversion(gon, centesimalMinute, 100), // gon to centesimal minute

                        new UnitScalarConversion(centesimalMinute, centesimalSecond, 100), // centesimal minute to centesimal second

                        new SexagesimalDmsToDecimalDegreesConversion(dms, degree9102), // sexagesimal dms to dd
                        new SexagesimalDmToDecimalDegreesConversion(dm, degree9102), // sexagesimal dm to dd 
                    }
                )
            );
        }

        [Obsolete]
        private static UnitScalarConversion CreateScalarConversion(ushort fromKey, ushort toKey, double factor) {
            Contract.Requires(!Double.IsNaN(factor));
            Contract.Requires(factor != 0);
            Contract.Ensures(Contract.Result<UnitScalarConversion>() != null);
            var from = Readers.GetByKey(fromKey);
            var to = Readers.GetByKey(toKey);
            Contract.Assume(from != null);
            Contract.Assume(to != null);
            return new UnitScalarConversion(from, to, factor);
        }

        [Obsolete]
        public static EpsgUnit Get(int code) {
            return code >= 0 && code < ushort.MaxValue
                ? Readers.GetByKey((ushort)code)
                : null;
        }

        [Obsolete]
        public static IEnumerable<EpsgUnit> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgUnit>>() != null);
                return Readers.ReadAllValues();
            }
        }

        private readonly ushort _code;

        internal EpsgUnit(ushort code, string name, string type, double factorB, double factorC) {
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

        private IUnitConversion<double> BuildConversionToBase() {
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
            Contract.Ensures(Contract.Result<IUnit>() != null);
            IUnit unit;
            if (StringComparer.OrdinalIgnoreCase.Equals("Angle", Type))
                unit = Readers.ReaderAngle.GetByKey(9101);
            else if (StringComparer.OrdinalIgnoreCase.Equals("Length", Type))
                unit = Readers.ReaderLength.GetByKey(9001);
            else if (StringComparer.OrdinalIgnoreCase.Equals("Scale", Type))
                unit = Readers.ReaderScale.GetByKey(9201);
            else if (StringComparer.OrdinalIgnoreCase.Equals("Time", Type))
                unit = Readers.ReaderTime.GetByKey(1040);
            else
                unit = null; // TODO: consider, Readers.ReadAllValues().Where(x => x.Type == Type).Single(x => x.FactorB == 1 && x.FactorC == 1);

            if (unit != null)
                return unit;
            throw new NotSupportedException();
        }

        public IUnitConversionMap<double> ConversionMap {
            get {
                Contract.Ensures(Contract.Result<IUnitConversionMap<double>>() != null);
                return AllConversionMap;
            }
        }

        public override string ToString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return Name;
        }
    }
}
