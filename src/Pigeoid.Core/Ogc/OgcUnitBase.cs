using System;
using System.Diagnostics.Contracts;
using System.Threading;
using Pigeoid.Contracts;
using Pigeoid.Unit;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// The base OGC unit of measure class. All units are relative to a single base unit by some factor.
    /// </summary>
    public abstract class OgcUnitBase : OgcNamedAuthorityBoundEntity, IUnit
    {
        private readonly Lazy<IUnitConversionMap<double>> _referenceConversionMap;

        protected OgcUnitBase(string name, double factor)
            : this(name, factor, null) { Contract.Requires(name != null);}

        protected OgcUnitBase(string name, double factor, IAuthorityTag authority)
            : base(name, authority) {
            Contract.Requires(name != null);
            Factor = factor;
            _referenceConversionMap = new Lazy<IUnitConversionMap<double>>(CreateReferenceConversionMap, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public abstract string Type { get; }

        public abstract IUnit ReferenceUnit { get; }

        string IUnit.Name {
            get { return Name; }
        }

        /// <summary>
        /// The conversion factor for the reference unit.
        /// </summary>
        public double Factor { get; private set; }

        private IUnitConversion<double> CreateForwardReferenceOperation() {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (1.0 == Factor)
                return new UnitUnityConversion(this, ReferenceUnit);
            return new UnitScalarConversion(this, ReferenceUnit, Factor);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        private IUnitConversionMap<double> CreateReferenceConversionMap() {
            if (UnitEqualityComparer.Default.Equals(this, ReferenceUnit))
                return null;
            return new BinaryUnitConversionMap(CreateForwardReferenceOperation());
        }

        public IUnitConversionMap<double> ConversionMap {
            get { return _referenceConversionMap.Value; }
        }

    }
}
