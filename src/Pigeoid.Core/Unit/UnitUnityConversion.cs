using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid.Unit
{
    public class UnitUnityConversion : IUnitScalarConversion<double>
    {

        public UnitUnityConversion(IUnit from, IUnit to) {
            if (null == from) throw new ArgumentNullException("from");
            if (null == to) throw new ArgumentNullException("to");
            Contract.EndContractBlock();
            From = from;
            To = to;
        }

        private void CodeContractInvariants() {
            Contract.Invariant(From != null);
            Contract.Invariant(To != null);
        }

        public double Factor { get { return 1.0; } }

        public IUnit From { get; private set; }

        public IUnit To { get; private set; }

        public void TransformValues(double[] values) {
            // Do nothing
        }

        public double TransformValue(double value) { return value; }

        public IEnumerable<double> TransformValues(IEnumerable<double> values) {
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<IEnumerable<double>>() != null);
            return values;
        }

        bool ITransformation.HasInverse { get { return true; } }

        public IUnitScalarConversion<double> GetInverse() {
            Contract.Ensures(Contract.Result<IUnitScalarConversion<double>>() != null);
            return new UnitUnityConversion(To, From);
        }

        IUnitConversion<double> IUnitConversion<double>.GetInverse() { return GetInverse(); }

        ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

        ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

        ITransformation ITransformation.GetInverse() { return GetInverse(); }

    }
}
