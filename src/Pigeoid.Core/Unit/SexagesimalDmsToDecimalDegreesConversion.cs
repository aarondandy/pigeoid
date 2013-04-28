using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur.Contracts;

namespace Pigeoid.Unit
{
    /// <summary>
    /// Converts from sexagesimal DDD.MMSSs format to decimal degrees.
    /// </summary>
    public class SexagesimalDmsToDecimalDegreesConversion : IUnitConversion<double>
    {

        private class Inverse : IUnitConversion<double>
        {

            private readonly SexagesimalDmsToDecimalDegreesConversion _core;

            public Inverse(SexagesimalDmsToDecimalDegreesConversion core) {
                Contract.Requires(core != null);
                _core = core;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_core != null);
            }

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

            public double TransformValue(double value) {
                return (double)TransformValue((decimal)value);
            }

            public decimal TransformValue(decimal value) {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                var result = (decimal)(int)value; // get the whole degrees
                value -= result; // take the whole degrees out
                if (0 == value)
                    return result;

                value *= 60; // the number of minutes (including fraction)
                var wholeMinutes = (int)value;
                value -= wholeMinutes; // take the whole minutes out, leaving the fraction of the minutes
                result += wholeMinutes / 100m;
                if (0 == value)
                    return result;

                return result + value * 0.006m; // add on the remaining fraction of a minute as a seconds
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            public void TransformValues(double[] values) {
                for (int i = 0; i < values.Length; i++)
                    values[i] = TransformValue(values[i]);
            }

            public IEnumerable<double> TransformValues(IEnumerable<double> values) {
                Contract.Ensures(Contract.Result<IEnumerable<double>>() != null);
                return values.Select(TransformValue);
            }

            public IUnitConversion<double> GetInverse() {
                Contract.Ensures(Contract.Result<IUnitConversion<double>>() != null);
                return _core;
            }

            ITransformation<double> ITransformation<double>.GetInverse() {
                return _core;
            }

            ITransformation<double, double> ITransformation<double, double>.GetInverse() {
                return _core;
            }

            ITransformation ITransformation.GetInverse() {
                return _core;
            }

            bool ITransformation.HasInverse {
                get { return true; }
            }
        }

        private readonly IUnit _from;
        private readonly IUnit _to;
        private readonly Inverse _inverse;

        public SexagesimalDmsToDecimalDegreesConversion(IUnit from, IUnit to) {
            if (null == from) throw new ArgumentNullException("from");
            if (null == to) throw new ArgumentNullException("to");
            Contract.EndContractBlock();
            _from = from;
            _to = to;
            _inverse = new Inverse(this);
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_from != null);
            Contract.Invariant(_to != null);
            Contract.Invariant(_inverse != null);
        }

        public IUnit From {
            [Pure] get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                return _from;
            }
        }

        public IUnit To {
            [Pure] get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                return _to;
            }
        }

        public double TransformValue(double value) {
            return (double)TransformValue((decimal)value);
        }

        public decimal TransformValue(decimal value) {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            var result = (decimal)(int)value; // get the whole degrees
            value = (value - result) * 100; // remove the whole degrees from the value
            if (0 == value)
                return result;

            var wholeMinutes = (int)value; // get the whole minutes
            result += (wholeMinutes / 60m); // add the whole minutes
            value = (value - wholeMinutes) * 100; // remove them from the value
            if (0 == value)
                return result;

            return result + (value / 3600m); // add on the seconds
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public void TransformValues(double[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }

        public IEnumerable<double> TransformValues(IEnumerable<double> values) {
            Contract.Ensures(Contract.Result<IEnumerable<double>>() != null);
            return values.Select(TransformValue);
        }

        public IUnitConversion<double> GetInverse() {
            Contract.Ensures(Contract.Result<IUnitConversion<double>>() != null);
            return _inverse;
        }

        ITransformation<double> ITransformation<double>.GetInverse() {
            return _inverse;
        }

        ITransformation<double, double> ITransformation<double, double>.GetInverse() {
            return _inverse;
        }

        ITransformation ITransformation.GetInverse() {
            return _inverse;
        }

        bool ITransformation.HasInverse {
            [Pure] get { return true; }
        }
    }
}
