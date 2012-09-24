using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class CoordinateOperationInfoInverse : IParameterizedCoordinateOperationInfo
	{

		public ICoordinateOperationInfo Core { get; private set; }

		public CoordinateOperationInfoInverse(ICoordinateOperationInfo core) {
			if(null == core)
				throw new ArgumentNullException("core");

			Core = core;
		}

		public string Name { get { return "Inverse " + Core.Name; } }

		public IEnumerable<INamedParameter> Parameters {
			get {
				var parameterizedOperationInfo = Core as IParameterizedCoordinateOperationInfo;
				return null != parameterizedOperationInfo
					? parameterizedOperationInfo.Parameters
					: Enumerable.Empty<INamedParameter>();
			}
		}

		public bool HasInverse { get { return true; } }

		public ICoordinateOperationInfo GetInverse() { return Core; }

		public bool IsInverseOfDefinition { get { return true; } }

		public ICoordinateOperationMethodInfo Method {
			get {
				return Core is IParameterizedCoordinateOperationInfo
					? (Core as IParameterizedCoordinateOperationInfo).Method
					: null;
			}
		}
	}

}
