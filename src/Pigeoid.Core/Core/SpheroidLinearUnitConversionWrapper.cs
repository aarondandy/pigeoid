using System;
using System.Diagnostics.Contracts;
using Pigeoid.Unit;
using Vertesaur.Contracts;

namespace Pigeoid
{
    public class SpheroidLinearUnitConversionWrapper : ISpheroidInfo
    {

        private readonly string _name;
        private readonly ISpheroid<double> _shapeData;

        public SpheroidLinearUnitConversionWrapper(ISpheroidInfo spheroid, IUnitConversion<double> conversion)
            : this(spheroid, conversion.To, conversion.TransformValue(spheroid.A), conversion.TransformValue(spheroid.B)) {
            Contract.Requires(spheroid != null);
            Contract.Requires(conversion != null);
        }

        public SpheroidLinearUnitConversionWrapper(ISpheroidInfo spheroid, IUnit unit, double a, double b)
            : this(spheroid.Name, unit, spheroid, a, b) {
            Contract.Requires(spheroid != null);
            Contract.Requires(unit != null);
        }

        public SpheroidLinearUnitConversionWrapper(string name, IUnit unit, ISpheroid<double> shapeData, double a, double b) {
            if(unit == null) throw new ArgumentNullException("unit");
            if(shapeData == null) throw new ArgumentNullException("shapeData");
            Contract.EndContractBlock();
            _name = name;
            _shapeData = shapeData;
            AxisUnit = unit;
            A = a;
            B = b;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(AxisUnit != null);
        }

        public string Name {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                if (String.IsNullOrWhiteSpace(_name))
                    return "converted to " + AxisUnit;
                return _name + " converted to " + AxisUnit;
            }
        }

        public IUnit AxisUnit { get; private set; }

        public double A { get; private set; }

        public double B { get; private set; }

        public double E {
            get { return _shapeData.E; }
        }

        public double ESecond {
            get { return _shapeData.ESecond; }
        }

        public double ESecondSquared {
            get { return _shapeData.ESecondSquared; }
        }

        public double ESquared {
            get { return _shapeData.ESquared; }
        }

        public double F {
            get { return _shapeData.F; }
        }

        public double InvF {
            get { return _shapeData.InvF; }
        }

        public IAuthorityTag Authority {
            get {
                return null; // NOTE: whatever authority tag it used to have, it no longer has!
            }
        }
    }
}
