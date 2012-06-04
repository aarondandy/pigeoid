// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pigeoid.Epsg
{
	public class EpsgCoordinateOperationConcatenatedInfo
	{

		private readonly ushort _code;
		private readonly ushort _sourceCrsCode;
		private readonly ushort _targetCrsCode;
		private readonly ushort _areaCode;
		private readonly bool _deprecated;
		private readonly string _name;
		private readonly ushort[] _stepCodes;

		internal EpsgCoordinateOperationConcatenatedInfo(
			ushort code, ushort sourceCrsCode, ushort targetCrsCode,
			ushort areaCode, bool deprecated, string name,
			ushort[] stepCodes
		) {
			_code = code;
			_sourceCrsCode = sourceCrsCode;
			_targetCrsCode = targetCrsCode;
			_areaCode = areaCode;
			_deprecated = deprecated;
			_name = name;
			_stepCodes = stepCodes;
		}

		public int Code { get { return _code; } }

		[Obsolete("TODO: maybe remove this?")]
		public int SourceCrsCode { get { return _sourceCrsCode; } }

		[Obsolete("TODO: maybe remove this?")]
		public int TargetCrsCode { get { return _targetCrsCode; } }

		public EpsgCrs SourceCrs { get { return EpsgCrs.Get(_sourceCrsCode); } }

		public EpsgCrs TargetCrs { get { return EpsgCrs.Get(_targetCrsCode); } }

		public EpsgArea Area { get { return EpsgArea.Get(_areaCode); } }

		public string Name { get { return _name; } }

		public bool Deprecated { get { return _deprecated; } }

		public IEnumerable<EpsgCoordinateOperationInfo> Steps { get {
			return _stepCodes.Select(x => EpsgCoordinateOperationInfoRepository.GetOperationInfo(x));
		}}

	}
}
