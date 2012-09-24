using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class UomScalarConversion : IUomScalarConversion<double>
	{

		private class Inverse : IUomScalarConversion<double>
		{
			private readonly UomScalarConversion _core;
			private readonly double _factor;
			private readonly double _inverseFactor;

			public Inverse(UomScalarConversion core) {
				_core = core;
				_factor = _core.Factor;
				_inverseFactor = 1.0 / _factor;
			}

			public double Factor { get { return _inverseFactor; } }

			public IUom From { get { return _core.To; } }

			public IUom To { get { return _core.From; } }

			public void TransformValues(double[] values) {
				for (int i = 0; i < values.Length; i++)
					values[i] /= _factor;
			}

			public double TransformValue(double value) { return value / _factor; }

			public IEnumerable<double> TransformValues(IEnumerable<double> values) { return values.Select(x => x / _factor); }

			public bool HasInverse { get { return true; } }

			public IUomScalarConversion<double> GetInverse() { return _core; }

			IUomConversion<double> IUomConversion<double>.GetInverse() { return GetInverse(); }

			ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

			ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

			ITransformation ITransformation.GetInverse() { return GetInverse(); }

		}

		private readonly IUom _from;
		private readonly IUom _to;
		private readonly double _factor;

		public UomScalarConversion(IUom from, IUom to, double factor) {
			if(null == from)
				throw new ArgumentNullException("from");
			if(null == to)
				throw new ArgumentNullException("to");

			_from = from;
			_to = to;
			_factor = factor;
		}

		public double Factor {get { return _factor; } }

		public IUom From { get { return _from; } }

		public IUom To { get { return _to; } }

		public void TransformValues(double[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] *= _factor;
		}

		public double TransformValue(double value) { return value * _factor; }

		public IEnumerable<double> TransformValues(IEnumerable<double> values) { return values.Select(x => x * _factor); }

// ReSharper disable CompareOfFloatsByEqualityOperator
		public bool HasInverse { get { return 0 != _factor; } }
// ReSharper restore CompareOfFloatsByEqualityOperator

		public IUomScalarConversion<double> GetInverse() { return new Inverse(this); }

		IUomConversion<double> IUomConversion<double>.GetInverse() { return GetInverse(); }

		ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

		ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

		ITransformation ITransformation.GetInverse() { return GetInverse(); }

	}
}
