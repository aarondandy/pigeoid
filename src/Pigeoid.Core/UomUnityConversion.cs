using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class UomUnityConversion : IUomScalarConversion<double>
	{

		private readonly IUom _from;
		private readonly IUom _to;

		public UomUnityConversion([NotNull] IUom from, [NotNull] IUom to) {
			if(null == from)
				throw new ArgumentNullException("from");
			if(null == to)
				throw new ArgumentNullException("to");

			_from = from;
			_to = to;
		}

		public double Factor { get { return 1.0; } }

		public IUom From { [ContractAnnotation("=>notnull")] get { return _from; } }

		public IUom To { [ContractAnnotation("=>notnull")] get { return _to; } }

		public void TransformValues(double[] values) {
			// Do nothing
		}

		public double TransformValue(double value) { return value; }

		[ContractAnnotation("null=>null; notnull=>notnull")] 
		public IEnumerable<double> TransformValues(IEnumerable<double> values) { return values; }

		public bool HasInverse { [ContractAnnotation("=>true")] get { return true; } }

		[ContractAnnotation("=>notnull")]
		public IUomScalarConversion<double> GetInverse() { return new UomUnityConversion(To, From); }

		IUomConversion<double> IUomConversion<double>.GetInverse() { return GetInverse(); }

		ITransformation<double> ITransformation<double>.GetInverse() { return GetInverse(); }

		ITransformation<double, double> ITransformation<double, double>.GetInverse() { return GetInverse(); }

		ITransformation ITransformation.GetInverse() { return GetInverse(); }

	}
}
