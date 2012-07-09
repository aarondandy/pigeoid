using System;
using System.Collections.Generic;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgCoordinateOperationInverse : ICoordinateOperationInfo
	{

		private readonly EpsgCoordinateOperationInfoBase _core;

		internal EpsgCoordinateOperationInverse(EpsgCoordinateOperationInfoBase core) {
			if(null == core)
				throw new ArgumentNullException("core");
			_core = core;
		}

		public string Name {
			get { return "Inverse " + _core.Name; }
		}

		public IEnumerable<INamedParameter> Parameters {
			get { return _core.Parameters; }
		}

		public bool HasInverse {
			get { return true; }
		}

		public EpsgCoordinateOperationInfoBase GetInverse() {
			return _core;
		}

		ICoordinateOperationInfo ICoordinateOperationInfo.GetInverse() {
			return GetInverse();
		}
	}
}
