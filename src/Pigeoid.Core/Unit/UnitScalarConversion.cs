using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;
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

			public Inverse([NotNull] UnitScalarConversion core) {
				_core = core;
				_factor = _core.Factor;
				_inverseFactor = 1.0 / _factor;
			}

			public double Factor { get { return _inverseFactor; } }

			public IUnit From { [ContractAnnotation("=>notnull")] get { return _core.To; } }

			public IUnit To { [ContractAnnotation("=>notnull")] get { return _core.From; } }

			public void TransformValues([NotNull] double[] values) {
				for (int i = 0; i < values.Length; i++)
					values[i] /= _factor;
			}

			public double TransformValue(double value) { return value / _factor; }

			[ContractAnnotation("=>notnull")] 
			public IEnumerable<double> TransformValues([NotNull] IEnumerable<double> values) { return values.Select(x => x / _factor); }

			public bool HasInverse { [ContractAnnotation("=>true")] get { return true; } }

			[ContractAnnotation("=>notnull")] 
			public IUnitScalarConversion<double> GetInverse() { return _core; }

			IUnitConversion<double> IUnitConversion<double>.GetInverse() { return GetInverse(); }

			ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

			ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

			ITransformation ITransformation.GetInverse() { return GetInverse(); }

		}

		private readonly IUnit _from;
		private readonly IUnit _to;
		private readonly double _factor;

		public UnitScalarConversion([NotNull] IUnit from, [NotNull] IUnit to, double factor) {
			if(null == from)
				throw new ArgumentNullException("from");
			if(null == to)
				throw new ArgumentNullException("to");

			_from = from;
			_to = to;
			_factor = factor;
		}

		public double Factor {get { return _factor; } }

		public IUnit From { [ContractAnnotation("=>notnull")] get { return _from; } }

		public IUnit To { [ContractAnnotation("=>notnull")] get { return _to; } }

		public void TransformValues([NotNull] double[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] *= _factor;
		}

		public double TransformValue(double value) { return value * _factor; }

		[ContractAnnotation("=>notnull")] 
		public IEnumerable<double> TransformValues([NotNull] IEnumerable<double> values) { return values.Select(x => x * _factor); }

// ReSharper disable CompareOfFloatsByEqualityOperator
		public bool HasInverse { get { return 0 != _factor; } }
// ReSharper restore CompareOfFloatsByEqualityOperator

		[ContractAnnotation("=>notnull")] 
		public IUnitScalarConversion<double> GetInverse() { return new Inverse(this); }

		IUnitConversion<double> IUnitConversion<double>.GetInverse() { return GetInverse(); }

		ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

		ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

		ITransformation ITransformation.GetInverse() { return GetInverse(); }

	}
}
