using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class CoordinateOperationInfoInverse : IParameterizedCoordinateOperationInfo
	{

		public ICoordinateOperationInfo Core { [ContractAnnotation("=>notnull")] get; private set; }

		public IParameterizedCoordinateOperationInfo ParameterizedCore { get { return Core as IParameterizedCoordinateOperationInfo; } }

		public CoordinateOperationInfoInverse([NotNull] ICoordinateOperationInfo core) {
			if(null == core)
				throw new ArgumentNullException("core");

			Core = core;
		}

		public string Name { get { return "Inverse " + Core.Name; } }

		public IEnumerable<INamedParameter> Parameters {
			get {
				var parameterizedOperationInfo = ParameterizedCore;
				return null != parameterizedOperationInfo
					? parameterizedOperationInfo.Parameters
					: Enumerable.Empty<INamedParameter>();
			}
		}

		public bool HasInverse { [ContractAnnotation("=>true")]get { return true; } }

		[ContractAnnotation("=>notnull")]
		public ICoordinateOperationInfo GetInverse() { return Core; }

		public bool IsInverseOfDefinition { [ContractAnnotation("=>true")] get { return true; } }

		public ICoordinateOperationMethodInfo Method {
			get{
				var paramOp = ParameterizedCore;
				return null != paramOp
					? paramOp.Method
					: null;
			}
		}
	}

}
