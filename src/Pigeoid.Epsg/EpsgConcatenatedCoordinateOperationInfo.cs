// TODO: source header

using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgConcatenatedCoordinateOperationInfo : EpsgCoordinateOperationInfoBase, IConcatenatedCoordinateOperationInfo
	{

		private readonly ushort _sourceCrsCode;
		private readonly ushort _targetCrsCode;

		private readonly ushort[] _stepCodes;

		internal EpsgConcatenatedCoordinateOperationInfo(
			ushort code, ushort sourceCrsCode, ushort targetCrsCode,
			ushort areaCode, bool deprecated, string name,
			ushort[] stepCodes
		)
			: base(code,areaCode,deprecated,name) {
			_sourceCrsCode = sourceCrsCode;
			_targetCrsCode = targetCrsCode;
			_stepCodes = stepCodes;
		}

		public int SourceCrsCode { get { return _sourceCrsCode; } }

		public int TargetCrsCode { get { return _targetCrsCode; } }

		public EpsgCrs SourceCrs { get { return EpsgCrs.Get(_sourceCrsCode); } }

		public EpsgCrs TargetCrs { get { return EpsgCrs.Get(_targetCrsCode); } }

		public IEnumerable<EpsgCoordinateOperationInfo> Steps {
			get{
				return _stepCodes
					.Select(x => EpsgCoordinateOperationInfoRepository.GetOperationInfo(x));
			}
		}

		IEnumerable<ICoordinateOperationInfo> IConcatenatedCoordinateOperationInfo.Steps { get { return Steps; } }

		public override bool HasInverse { get { return Steps.All(step => step.HasInverse); } }
	}
}
