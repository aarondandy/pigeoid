using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid.Unit
{
	public class UnitUnityConversion : IUnitScalarConversion<double>
	{

		private readonly IUnit _from;
		private readonly IUnit _to;

		public UnitUnityConversion([NotNull] IUnit from, [NotNull] IUnit to) {
			if(null == from)
				throw new ArgumentNullException("from");
			if(null == to)
				throw new ArgumentNullException("to");

			_from = from;
			_to = to;
		}

		public double Factor { get { return 1.0; } }

		public IUnit From { [ContractAnnotation("=>notnull")] get { return _from; } }

		public IUnit To { [ContractAnnotation("=>notnull")] get { return _to; } }

		public void TransformValues(double[] values) {
			// Do nothing
		}

		public double TransformValue(double value) { return value; }

		[ContractAnnotation("null=>null; notnull=>notnull")] 
		public IEnumerable<double> TransformValues(IEnumerable<double> values) { return values; }

		public bool HasInverse { [ContractAnnotation("=>true")] get { return true; } }

		[ContractAnnotation("=>notnull")]
		public IUnitScalarConversion<double> GetInverse() { return new UnitUnityConversion(To, From); }

		IUnitConversion<double> IUnitConversion<double>.GetInverse() { return GetInverse(); }

		ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

		ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

		ITransformation ITransformation.GetInverse() { return GetInverse(); }

	}
}
