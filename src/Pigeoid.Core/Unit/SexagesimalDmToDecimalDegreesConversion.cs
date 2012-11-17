using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid.Unit
{

	/// <summary>
	/// Converts from sexagesimal DDD.MMm format to decimal degrees.
	/// </summary>
	public class SexagesimalDmToDecimalDegreesConversion : IUnitConversion<double>
	{

		private class Inverse : IUnitConversion<double>
		{

			private readonly SexagesimalDmToDecimalDegreesConversion _core;

			public Inverse([NotNull] SexagesimalDmToDecimalDegreesConversion core) {
				_core = core;
			}

			public IUnit From {
				get { return _core.To; }
			}

			public IUnit To {
				get { return _core.From; }
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

				return result + (value * 0.6m);
				// ReSharper restore CompareOfFloatsByEqualityOperator
			}

			public void TransformValues([NotNull] double[] values) {
				for (int i = 0; i < values.Length; i++)
					values[i] = TransformValue(values[i]);
			}

			public IEnumerable<double> TransformValues(IEnumerable<double> values) {
				return values.Select(TransformValue);
			}

			public IUnitConversion<double> GetInverse() {
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

			public bool HasInverse {
				get { throw new NotImplementedException(); }
			}
		}

		private readonly IUnit _from;
		private readonly IUnit _to;
		private readonly Inverse _inverse;

		public SexagesimalDmToDecimalDegreesConversion([NotNull] IUnit from, [NotNull] IUnit to) {
			if(null == from)
				throw new ArgumentNullException("from");
			if(null == to)
				throw new ArgumentNullException("to");

			_from = from;
			_to = to;
			_inverse = new Inverse(this);
		}

		[NotNull] public IUnit From {
			[Pure] get { return _from; }
		}

		[NotNull] public IUnit To {
			[Pure] get { return _to; }
		}

		public double TransformValue(double value) {
			return (double)TransformValue((decimal)value);
		}

		public decimal TransformValue(decimal value) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			var result = (decimal)(int)value; // get the whole degrees
			value -= result; // remove the whole degrees from the value
			if (0 == value)
				return result;

			return result + (value / 0.6m); // add on the minutes
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public void TransformValues([NotNull] double[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}

		public IEnumerable<double> TransformValues(IEnumerable<double> values) {
			return values.Select(TransformValue);
		}

		[NotNull] public IUnitConversion<double> GetInverse() {
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

		public bool HasInverse {
			[Pure, ContractAnnotation("=>true")] get { return true; }
		}
	}
}
