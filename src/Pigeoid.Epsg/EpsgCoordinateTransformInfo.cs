// TODO: source header

using System;

namespace Pigeoid.Epsg
{

	public class EpsgCoordinateTransformInfo : EpsgCoordinateOperationInfo
	{

		private readonly ushort _sourceCrsCode;
		private readonly ushort _targetCrsCode;
		private readonly double _accuracy;

		internal EpsgCoordinateTransformInfo(
			ushort code,
			ushort sourceCrsCode, ushort targetCrsCode,
			ushort opMethodCode,
			double accuracy,
			ushort areaCode, bool deprecated, string name
		) : base(code, opMethodCode, areaCode, deprecated, name) {
			_sourceCrsCode = sourceCrsCode;
			_targetCrsCode = targetCrsCode;
			_accuracy = accuracy;
		}

		[Obsolete("TODO: maybe remove this?")]
		public int SourceCrsCode { get { return _sourceCrsCode; } }

		[Obsolete("TODO: maybe remove this?")]
		public int TargetCrsCode { get { return _targetCrsCode; } }

		public EpsgCrs SourceCrs { get { return EpsgCrs.Get(_sourceCrsCode); } }

		public EpsgCrs TargetCrs { get { return EpsgCrs.Get(_targetCrsCode); } }

		public double Accuracy { get { return _accuracy; } }

	}

}
