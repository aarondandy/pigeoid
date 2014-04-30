using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.Unit
{
    public class ConcatenatedUnitConversion : IUnitConversion<double>
    {

        private readonly IUnitConversion<double>[] _conversions;

        /// <exception cref="System.ArgumentException">At least one conversion is required.</exception>
        public ConcatenatedUnitConversion(IEnumerable<IUnitConversion<double>> conversions)
            : this(conversions.ToArray())
        {
            Contract.Requires(conversions != null);
            Contract.Requires(conversions.Any());
            Contract.Requires(Contract.ForAll(conversions, x => x != null));
        }

        protected ConcatenatedUnitConversion(IUnitConversion<double>[] conversions) {
            if (null == conversions) throw new ArgumentNullException("conversions");
            if (conversions.Length < 1) throw new ArgumentException("At least one conversion is required.", "conversions");
            Contract.Requires(Contract.ForAll(conversions, x => x != null));

            if (conversions.Any(x => x == null))
                throw new ArgumentException("No null conversions are allowed", "conversions");

            Contract.Assume(Contract.ForAll(conversions, x => x != null));
            _conversions = conversions;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_conversions != null);
            Contract.Invariant(_conversions.Length >= 1);
            Contract.Invariant(Contract.ForAll(_conversions, x => x != null));
        }

        public IUnit From {
            get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                return _conversions[0].From;
            }
        }

        public IUnit To {
            get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                return _conversions[_conversions.Length - 1].To;
            }
        }

        public ReadOnlyCollection<IUnitConversion<double>> Conversions {
            get {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<IUnitConversion<double>>>() != null);
                return Array.AsReadOnly(_conversions);
            }
        }

        public void TransformValues(double[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }

        public double TransformValue(double value) {
            for (int i = 0; i < _conversions.Length; i++)
                value = _conversions[i].TransformValue(value);

            return value;
        }

        public IEnumerable<double> TransformValues(IEnumerable<double> values) {
            Contract.Ensures(Contract.Result<IEnumerable<double>>() != null);
            return values.Select(TransformValue);
        }

        public ConcatenatedUnitConversion GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ConcatenatedUnitConversion>() != null);
            var inverse = Array.ConvertAll(_conversions, x => x.GetInverse());
            Array.Reverse(inverse);
            Contract.Assume(Contract.ForAll(inverse, x => x != null));
            return new ConcatenatedUnitConversion(inverse);
        }

        IUnitConversion<double> IUnitConversion<double>.GetInverse() {
            return GetInverse();
        }

        ITransformation<double> ITransformation<double>.GetInverse() {
            return GetInverse();
        }

        ITransformation<double, double> ITransformation<double, double>.GetInverse() {
            return GetInverse();
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        public bool HasInverse {
            [Pure] get {
                return _conversions.All(x => x.HasInverse);
            }
        }

        public Type[] GetInputTypes() {
            return new[] { typeof(double) };
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(double)
                ? new[] { inputType }
                : ArrayUtil<Type>.Empty;
        }

        public object TransformValue(object value) {
            return TransformValue((double)value);
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }

    }
}
