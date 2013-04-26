using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Unit
{
    public class UnitScalarConversion : IUnitScalarConversion<double>
    {

        private class Inverse : IUnitScalarConversion<double>
        {
            private readonly UnitScalarConversion _core;
            private readonly double _factor;
            private readonly double _inverseFactor;

            public Inverse(UnitScalarConversion core) {
                Contract.Requires(core != null);
                _core = core;
                _factor = _core.Factor;
                _inverseFactor = 1.0 / _factor;
            }

            private void CodeContractInvariants() {
                Contract.Invariant(_core != null);
            }

            public double Factor { get { return _inverseFactor; } }

            public IUnit From {
                get {
                    Contract.Ensures(Contract.Result<IUnit>() != null);
                    return _core.To;
                }
            }

            public IUnit To {
                get {
                    Contract.Ensures(Contract.Result<IUnit>() != null);
                    return _core.From;
                }
            }

            public void TransformValues(double[] values) {
                Contract.Requires(values != null);
                for (int i = 0; i < values.Length; i++)
                    values[i] /= _factor;
            }

            public double TransformValue(double value) {
                return value / _factor;
            }

            public IEnumerable<double> TransformValues(IEnumerable<double> values) {
                Contract.Requires(values != null);
                Contract.Ensures(Contract.Result<IEnumerable<double>>() != null);
                return values.Select(x => x / _factor);
            }

            bool ITransformation.HasInverse { get { return true; } }

            public IUnitScalarConversion<double> GetInverse() {
                Contract.Ensures(Contract.Result<IUnitScalarConversion<double>>() != null);
                return _core;
            }

            IUnitConversion<double> IUnitConversion<double>.GetInverse() { return GetInverse(); }

            ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

            ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

            ITransformation ITransformation.GetInverse() { return GetInverse(); }

        }

        private readonly double _factor;

        public UnitScalarConversion(IUnit from, IUnit to, double factor) {
            if (null == from) throw new ArgumentNullException("from");
            if (null == to) throw new ArgumentNullException("to");
            Contract.EndContractBlock();
            From = from;
            To = to;
            _factor = factor;
        }

        private void CodeContractInvariants() {
            Contract.Invariant(From != null);
            Contract.Invariant(To != null);
        }

        public double Factor { get { return _factor; } }

        public IUnit From { get; private set; }

        public IUnit To { get; private set; }

        public void TransformValues(double[] values) {
            Contract.Requires(values != null);
            for (int i = 0; i < values.Length; i++)
                values[i] *= _factor;
        }

        public double TransformValue(double value) {
            return value * _factor;
        }

        public IEnumerable<double> TransformValues(IEnumerable<double> values) {
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<IEnumerable<double>>() != null);
            return values.Select(x => x * _factor);
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public bool HasInverse {
            get {
                return 0 != _factor && !Double.IsNaN(_factor);
            }
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

        public IUnitScalarConversion<double> GetInverse() {
            if(!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<IUnitScalarConversion<double>>() != null);
            return new Inverse(this);
        }

        IUnitConversion<double> IUnitConversion<double>.GetInverse() { return GetInverse(); }

        ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

        ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

        ITransformation ITransformation.GetInverse() { return GetInverse(); }

    }
}
