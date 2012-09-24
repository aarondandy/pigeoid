// TODO: source header

using System.Collections.Generic;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{

	public class EpsgCoordinateOperationInfo : EpsgCoordinateOperationInfoBase, IParameterizedCoordinateOperationInfo
	{

		private readonly ushort _opMethodCode;

		internal EpsgCoordinateOperationInfo(ushort code, ushort opMethodCode, ushort areaCode, bool deprecated, string name)
			: base(code, areaCode, deprecated, name) {
			_opMethodCode = opMethodCode;
		}

		public EpsgCoordinateOperationMethodInfo Method { get { return EpsgCoordinateOperationMethodInfo.Get(_opMethodCode); } }

		ICoordinateOperationMethodInfo IParameterizedCoordinateOperationInfo.Method {
			get { return Method; }
		}

		public IEnumerable<INamedParameter> Parameters {
			get {
				var opMethodInfo = Method; // make a local copy to prevent another call to 'Get'
				return null != opMethodInfo ? opMethodInfo.GetOperationParameters(Code) : null;
			}
		}

		public override bool HasInverse {
			get { return Method.CanReverse; }
		}


		
	}

}
