using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class ConcatenatedCoordinateOperationInfo : ICoordinateOperationInfo
	{

		private readonly ICoordinateOperationInfo[] _coordinateOperations;

		public ConcatenatedCoordinateOperationInfo(IEnumerable<ICoordinateOperationInfo> coordinateOperations)
		{
			if(null == coordinateOperations)
				throw new ArgumentNullException("coordinateOperations");

			_coordinateOperations = coordinateOperations.ToArray();
			if(_coordinateOperations.Length == 0)
				throw new ArgumentException("coordinateOperations must have at least one element", "coordinateOperations");
		}

		string ICoordinateOperationInfo.Name {
			get { return "Concatenated Operation"; }
		}

		IEnumerable<INamedParameter> ICoordinateOperationInfo.Parameters {
			get { return Enumerable.Empty<INamedParameter>(); }
		}

		public bool HasInverse {
			get { return _coordinateOperations.All(x => x.HasInverse); }
		}

		public ICoordinateOperationInfo GetInverse() {
			return new ConcatenatedCoordinateOperationInfo(_coordinateOperations.Reverse().Select(x => x.GetInverse()));
		}
	}
}
