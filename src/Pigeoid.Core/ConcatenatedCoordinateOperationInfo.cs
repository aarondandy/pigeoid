using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class ConcatenatedCoordinateOperationInfo : IConcatenatedCoordinateOperationInfo
	{

		private class Inverse : IConcatenatedCoordinateOperationInfo {

			private readonly ConcatenatedCoordinateOperationInfo _core;

			public Inverse(ConcatenatedCoordinateOperationInfo core){
				_core = core;
			}

			public IEnumerable<ICoordinateOperationInfo> Steps{
				get { return _core.Steps.Reverse().Select(x => x.GetInverse()); }
			}

			string ICoordinateOperationInfo.Name { get { return "Inverse " + ((ICoordinateOperationInfo)_core).Name; } }

			IEnumerable<INamedParameter> ICoordinateOperationInfo.Parameters { get { return ((ICoordinateOperationInfo)_core).Parameters; } }

			public bool HasInverse { get { return true; } }

			public ICoordinateOperationInfo GetInverse() { return _core; }

			public bool IsInverseOfDefinition{ get { return true; } }

		}

		private readonly ReadOnlyCollection<ICoordinateOperationInfo> _coordinateOperations;

		public ConcatenatedCoordinateOperationInfo(IEnumerable<ICoordinateOperationInfo> coordinateOperations) {
			if(null == coordinateOperations)
				throw new ArgumentNullException("coordinateOperations");

			_coordinateOperations = Array.AsReadOnly(coordinateOperations.ToArray());
			if(_coordinateOperations.Count == 0)
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
			return new Inverse(this);
		}

		public IEnumerable<ICoordinateOperationInfo> Steps {
			get { return _coordinateOperations; }
		}

		public bool IsInverseOfDefinition {
			get { return false; }
		}

	}
}
