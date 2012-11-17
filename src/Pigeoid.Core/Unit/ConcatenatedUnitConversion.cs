using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid.Unit
{
	public class ConcatenatedUnitConversion : IUnitConversion<double>
	{

		private readonly IUnitConversion<double>[] _conversions;

		public ConcatenatedUnitConversion([NotNull] IEnumerable<IUnitConversion<double>> conversions) {
			if(null == conversions)
				throw new ArgumentNullException("conversions");
			_conversions = conversions.ToArray();
			if(_conversions.Length == 0)
				throw new ArgumentException("At least one conversion is required.", "conversions");
		}

		public IUnit From {
			get { return _conversions[0].From; }
		}

		public IUnit To {
			get { return _conversions[_conversions.Length-1].To; }
		}

		public ReadOnlyCollection<IUnitConversion<double>> Conversions { get { return Array.AsReadOnly(_conversions); } }

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
			return values.Select(TransformValue);
		}

		public ConcatenatedUnitConversion GetInverse() {
			if(!HasInverse)
				throw new InvalidOperationException("No inverse.");
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

		public bool HasInverse { get { return _conversions.All(x => x.HasInverse); } }
	}
}
