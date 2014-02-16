using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.Unit
{
    public class UnitRatioConversion : IUnitScalarConversion<double>
    {

        private delegate void TransformValueInPlaceFunction(ref double value);

        private readonly double _numerator;
        private readonly double _denominator;
        private readonly double _factor;
        private readonly TransformValueInPlaceFunction _transformInPlace;
        private readonly Func<double, double> _transform;

        public UnitRatioConversion(IUnit from, IUnit to, double numerator, double denominator) {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (null == from) throw new ArgumentNullException("from");
            if (null == to) throw new ArgumentNullException("to");
            if (0 == denominator || Double.IsNaN(denominator)) throw new ArgumentException("denominator must be non-zero", "denominator");
            Contract.EndContractBlock();

            From = from;
            To = to;
            _numerator = numerator;
            _denominator = denominator;
            _factor = _numerator / _denominator;

            if (1.0 == _numerator && 1.0 != _denominator) {
                _transform = Divide;
                _transformInPlace = DivideInPlace;
            }
            else {
                _transform = Multiply;
                _transformInPlace = MultiplyInPlace;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(From != null);
            Contract.Invariant(To != null);
            Contract.Invariant(_denominator != 0 && !Double.IsNaN(_denominator));
            Contract.Invariant(_transformInPlace != null);
            Contract.Invariant(_transform != null);
        }

        private void MultiplyInPlace(ref double value) {
            value *= _factor;
        }

        private double Multiply(double value) {
            return value * _factor;
        }

        private void DivideInPlace(ref double value) {
            value /= _denominator;
        }

        private double Divide(double value) {
            return value / _denominator;
        }

        public double Factor { get { return _factor; } }

        public IUnit From { get; private set; }

        public IUnit To { get; private set; }

        public void TransformValues(double[] values) {
            for (int i = 0; i < values.Length; i++)
                _transformInPlace(ref values[i]);
        }

        public double TransformValue(double value) {
            return _transform(value);
        }

        public IEnumerable<double> TransformValues(IEnumerable<double> values) {
            Contract.Ensures(Contract.Result<IEnumerable<double>>() != null);
            return values.Select(_transform);
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public bool HasInverse {
            [Pure] get {
                return 0 != _numerator
                    && !Double.IsNaN(_numerator);
            }
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

        public IUnitScalarConversion<double> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<IUnitScalarConversion<double>>() != null);
            return new UnitRatioConversion(To, From, _denominator, _numerator);
        }

        IUnitConversion<double> IUnitConversion<double>.GetInverse() {
            Contract.Assume(HasInverse);
            return GetInverse();
        }

        ITransformation<double> ITransformation<double>.GetInverse() {
            Contract.Assume(HasInverse);
            return GetInverse();
        }

        ITransformation<double, double> ITransformation<double, double>.GetInverse() {
            Contract.Assume(HasInverse);
            return GetInverse();
        }

        ITransformation ITransformation.GetInverse() {
            Contract.Assume(HasInverse);
            return GetInverse();
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
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

    }
}
