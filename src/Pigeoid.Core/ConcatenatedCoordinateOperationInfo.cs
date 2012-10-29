using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class ConcatenatedCoordinateOperationInfo : IConcatenatedCoordinateOperationInfo
	{

		private class Inverse : IConcatenatedCoordinateOperationInfo {

			private readonly ConcatenatedCoordinateOperationInfo _core;

			public Inverse([NotNull] ConcatenatedCoordinateOperationInfo core){
				_core = core;
			}

			public IEnumerable<ICoordinateOperationInfo> Steps{
				[ContractAnnotation("=>notnull")] get { return _core.Steps.Reverse().Select(x => x.GetInverse()); }
			}

			string ICoordinateOperationInfo.Name { get { return "Inverse " + ((ICoordinateOperationInfo)_core).Name; } }

			public bool HasInverse { [ContractAnnotation("=>true")] get { return true; } }

			[ContractAnnotation("=>notnull")]
			public ICoordinateOperationInfo GetInverse() { return _core; }

			public bool IsInverseOfDefinition { [ContractAnnotation("=>true")] get { return true; } }

		}

		private readonly ReadOnlyCollection<ICoordinateOperationInfo> _coordinateOperations;

		public ConcatenatedCoordinateOperationInfo([NotNull] IEnumerable<ICoordinateOperationInfo> coordinateOperations) {
			if(null == coordinateOperations)
				throw new ArgumentNullException("coordinateOperations");

			_coordinateOperations = Array.AsReadOnly(coordinateOperations.ToArray());
			if(_coordinateOperations.Count == 0)
				throw new ArgumentException("coordinateOperations must have at least one element", "coordinateOperations");
		}

		string ICoordinateOperationInfo.Name {
			get { return "Concatenated Operation"; }
		}

		public bool HasInverse {
			get { return _coordinateOperations.All(x => x.HasInverse); }
		}

		[ContractAnnotation("=>notnull")]
		public ICoordinateOperationInfo GetInverse() {
			return new Inverse(this);
		}

		public IEnumerable<ICoordinateOperationInfo> Steps {
			[ContractAnnotation("=>notnull")] get { return _coordinateOperations; }
		}

		public bool IsInverseOfDefinition {
			[ContractAnnotation("=>false")] get { return false; }
		}

	}
}
