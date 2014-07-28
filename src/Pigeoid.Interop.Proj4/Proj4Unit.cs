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

        private static readonly int[] KnownUnitCodes = new[] {
            9001,
            9002,
            9003,
            9005,
            9014,
            9030,
            9031,
            9033,
            9034,
            9035,
            9036,
            9037,
            9038,
            9039,
            9040,
            9041,
            9042,
            9043,
            9050,
            9051,
            9052,
            9053,
            9060,
            9061,
            9062,
            9063,
            9070,
            9080,
            9081,
            9082,
            9083,
            9084,
            9085,
            9086,
            9087,
            9093,
            9094,
            9095,
            9096,
            9097,
            9098,
            9099,
            9204,
            9205,
            9206,
            9207,
            9208,
            9209,
            9210,
            9211,
            9300,
            9301,
            9302
        };
        private static readonly LinearUnit[] KnownUnits;
        private static readonly LinearUnit MetersUnit;
        private static readonly Proj4LinearUnit MetersUnitWrapped;

        public static Proj4LinearUnit Meter {
            get { return MetersUnitWrapped; }
        }

        static Proj4LinearUnit() {
            KnownUnits = Array.ConvertAll(KnownUnitCodes, uomCode => {
                var result = new LinearUnit();
                result.ReadCode(uomCode);
                return result;
            });
            MetersUnit = KnownUnits.Single(x => x.Meters == 1.0);
            MetersUnitWrapped = new Proj4LinearUnit(MetersUnit);
        }

        public static LinearUnit ConvertToProj4(IUnit unit) {
            if (unit == null) throw new ArgumentNullException("unit");
            Contract.EndContractBlock();

            var conversion = SimpleUnitConversionGenerator.FindConversion(unit, MetersUnitWrapped);
            if(conversion != null){
                if(conversion is UnitUnityConversion)
                    return MetersUnit;
                var scalarConversion = conversion as IUnitScalarConversion<double>;
                if (scalarConversion != null) {
                    var knownMatch = KnownUnits.FirstOrDefault(x => x.Meters == scalarConversion.Factor);
                    if (knownMatch != null)
                        return knownMatch;

                    var customResult = new LinearUnit();
                    customResult.Meters = scalarConversion.Factor;
                    customResult.Name = unit.Name;
                    return customResult;
                }
            }
            return new LinearUnit {
                Meters = 1,
                Name = unit.Name
            };
        }

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
                foreach (var knownUnit in KnownUnits) {
                    if (knownUnit.Meters == Core.Meters)
                        return knownUnit.Name;
                }

                foreach (var knownUnit in KnownUnits) {
                    if (UnitNameNormalizedComparer.Default.Equals(Core.Name, knownUnit.Name))
                        return Core.Meters.ToString("R");
                }
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
