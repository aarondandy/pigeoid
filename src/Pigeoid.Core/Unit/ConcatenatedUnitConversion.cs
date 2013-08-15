using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.Unit
{
    public class ConcatenatedUnitConversion : IUnitConversion<double>
    {

        private readonly IUnitConversion<double>[] _conversions;

        /// <exception cref="System.ArgumentException">At least one conversion is required.</exception>
        public ConcatenatedUnitConversion(IEnumerable<IUnitConversion<double>> conversions) {
            if (null == conversions) throw new ArgumentNullException("conversions");
            Contract.EndContractBlock();
            _conversions = conversions.ToArray();
            if (_conversions.Length == 0)
                throw new ArgumentException("At least one conversion is required.", "conversions");
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_conversions != null);
            Contract.Invariant(_conversions.Length >= 1);
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
            return new ConcatenatedUnitConversion(_conversions.Select(x => x.GetInverse()).Reverse());
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
    }
}
