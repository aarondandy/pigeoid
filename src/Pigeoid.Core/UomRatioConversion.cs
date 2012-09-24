using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class UomRatioConversion : IUomScalarConversion<double>
	{

		private delegate void TransformValueInPlaceFunction(ref double value);

		private readonly IUom _from;
		private readonly IUom _to;
		private readonly double _numerator;
		private readonly double _denominator;
		private readonly double _factor;
		private readonly TransformValueInPlaceFunction _transformInPlace;
		private readonly Func<double, double> _transform; 

		public UomRatioConversion(IUom from, IUom to, double numerator, double denominator) {
			if(null == from)
				throw new ArgumentNullException("from");
			if(null == to)
				throw new ArgumentNullException("to");

			_from = from;
			_to = to;
			_numerator = numerator;
			_denominator = denominator;
			_factor = _numerator / _denominator;

			if(1.0 == _numerator && 1.0 != _denominator) {
				_transform = Divide;
				_transformInPlace = DivideInPlace;
			}else {
				_transform = Multiply;
				_transformInPlace = MultiplyInPlace;
			}

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

		public IUom From { get { return _from; } }

		public IUom To { get { return _to; } }

		public void TransformValues(double[] values) {
			for (int i = 0; i < values.Length; i++)
				_transformInPlace(ref values[i]);
		}

		public double TransformValue(double value) {
			return _transform(value);
		}

		public IEnumerable<double> TransformValues(IEnumerable<double> values) { return values.Select(_transform); }

// ReSharper disable CompareOfFloatsByEqualityOperator
		public bool HasInverse { get { return 0 != _numerator; } }
// ReSharper restore CompareOfFloatsByEqualityOperator

		public IUomScalarConversion<double> GetInverse() { return new UomRatioConversion(To,From,_denominator,_numerator); }

		IUomConversion<double> IUomConversion<double>.GetInverse() { return GetInverse(); }

		ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

		ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

		ITransformation ITransformation.GetInverse() { return GetInverse(); }

	}
}
