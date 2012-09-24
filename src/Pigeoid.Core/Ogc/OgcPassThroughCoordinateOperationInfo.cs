using System;
using System.Collections.Generic;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	public class OgcPassThroughCoordinateOperationInfo : IPassThroughCoordinateOperationInfo
	{

		private readonly ICoordinateOperationInfo _core;

		public OgcPassThroughCoordinateOperationInfo(ICoordinateOperationInfo core, int firstAffectedOrdinate) {
			if(null == core)
				throw new ArgumentNullException("core");

			_core = core;
			FirstAffectedOrdinate = firstAffectedOrdinate;
		}

		public int FirstAffectedOrdinate { get; private set; }

		public IEnumerable<ICoordinateOperationInfo> Steps {
			get { return Array.AsReadOnly(new []{_core}); }
		}

		public string Name {
			get { return "Pass-through"; }
		}

		public bool HasInverse {
			get { return 0 == FirstAffectedOrdinate && _core.HasInverse; }
		}

		public ICoordinateOperationInfo GetInverse() {
			if(!HasInverse)
				throw new InvalidOperationException("No inverse.");
			return _core.GetInverse();
		}

		public bool IsInverseOfDefinition {
			get { return false; }
		}
	}
}
