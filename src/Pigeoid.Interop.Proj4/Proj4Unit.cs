using DotSpatial.Projections;
using Pigeoid.Ogc;
using Pigeoid.Unit;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Interop.Proj4
{
    public class Proj4LinearUnit : IUnit
    {

        public Proj4LinearUnit(LinearUnit core) {
            if (core == null) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            Core = core;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Core != null);
        }

        public LinearUnit Core { get; private set; }

        public string Name {
            get {
                return Core.Name;
            }
        }

        public string Type {
            get {
                return "length";
            }
        }

        public IUnitConversionMap<double> ConversionMap {
            get {
                if (UnitEqualityComparer.Default.Equals(this, OgcLinearUnit.DefaultMeter))
                    return null; // TODO: should this be a unity conversion?
                return new BinaryUnitConversionMap(
                    Core.Meters == 1.0
                    ? new UnitUnityConversion(this, OgcLinearUnit.DefaultMeter)
                    : (IUnitConversion<double>)new UnitScalarConversion(this, OgcLinearUnit.DefaultMeter, Core.Meters)
                );
            }
        }
    }

    public class Proj4AngularUnit : IUnit
    {
        public Proj4AngularUnit(AngularUnit core) {
            if (core == null) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            Core = core;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Core != null);
        }

        public AngularUnit Core { get; private set; }

        public string Name {
            get {
                return Core.Name;
            }
        }

        public string Type {
            get {
                return "angle";
            }
        }

        public IUnitConversionMap<double> ConversionMap {
            get {
                if (UnitEqualityComparer.Default.Equals(this, OgcAngularUnit.DefaultRadians))
                    return null; // TODO: should this be a unity conversion?
                return new BinaryUnitConversionMap(
                    Core.Radians == 1.0
                    ? new UnitUnityConversion(this, OgcAngularUnit.DefaultRadians)
                    : (IUnitConversion<double>)new UnitScalarConversion(this, OgcAngularUnit.DefaultRadians, Core.Radians)
                );
            }
        }
    }
}
