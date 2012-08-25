// TODO: source header

using System.Collections.Generic;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{

	public class EpsgCoordinateOperationInfo : EpsgCoordinateOperationInfoBase
	{

		private readonly ushort _opMethodCode;

		internal EpsgCoordinateOperationInfo(ushort code, ushort opMethodCode, ushort areaCode, bool deprecated, string name)
			: base(code, areaCode, deprecated, name) {
			_opMethodCode = opMethodCode;
		}

		public EpsgCoordinateOperationMethodInfo OperationMethodInfo { get { return EpsgCoordinateOperationMethodInfo.Get(_opMethodCode); } }

		public override IEnumerable<INamedParameter> Parameters {
			get {
				var opMethodInfo = OperationMethodInfo;
				return null != opMethodInfo ? opMethodInfo.GetOperationParameters(Code) : null;
			}
		}

		public override bool HasInverse {
			get { return OperationMethodInfo.CanReverse; }
		}
	}

}
