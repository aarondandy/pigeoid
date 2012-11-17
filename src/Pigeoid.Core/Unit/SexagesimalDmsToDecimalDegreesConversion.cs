using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;
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

			public Inverse([NotNull] SexagesimalDmsToDecimalDegreesConversion core) {
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

				value *= 60; // the number of minutes (including fraction)
				var wholeMinutes = (int)value;
				value -= wholeMinutes; // take the whole minutes out, leaving the fraction of the minutes
				result += wholeMinutes / 100m;
				if (0 == value)
					return result;

				return result + value * 0.006m; // add on the remaining fraction of a minute as a seconds
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
				get { return true; }
			}
		}

		private readonly IUnit _from;
		private readonly IUnit _to;
		private readonly Inverse _inverse;

		public SexagesimalDmsToDecimalDegreesConversion([NotNull] IUnit from, [NotNull] IUnit to) {
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

		public void TransformValues([NotNull] double[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}

		[NotNull] public IEnumerable<double> TransformValues([NotNull] IEnumerable<double> values) {
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
			[ContractAnnotation("=>true"), Pure] get { return true; }
		}
	}
}
