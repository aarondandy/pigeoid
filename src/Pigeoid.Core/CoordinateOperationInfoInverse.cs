using System;
using System.Collections.Generic;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class CoordinateOperationInfoInverse : ICoordinateOperationInfo
	{

		public ICoordinateOperationInfo Core { get; private set; }

		public CoordinateOperationInfoInverse(ICoordinateOperationInfo core)
		{
			if(null == core)
				throw new ArgumentNullException("core");

			Core = core;
		}

		public string Name { get { return "Inverse " + Core.Name; } }

		public IEnumerable<INamedParameter> Parameters { get { return Core.Parameters; } }

		public bool HasInverse { get { return true; } }

		public ICoordinateOperationInfo GetInverse() { return Core; }

		public bool IsInverseOfDefinition { get { return true; } }
	}

}
