// TODO: source header

using System.Collections.Generic;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{

	public class EpsgCoordinateOperationInfo :
		ICoordinateOperationInfo
	{

		

		protected readonly ushort _code;
		private readonly ushort _opMethodCode;
		private readonly ushort _areaCode;
		private readonly bool _deprecated;
		private readonly string _name;

		internal EpsgCoordinateOperationInfo(ushort code, ushort opMethodCode, ushort areaCode, bool deprecated, string name) {
			_code = code;
			_opMethodCode = opMethodCode;
			_areaCode = areaCode;
			_deprecated = deprecated;
			_name = name;
		}

		public int Code { get { return _code; } }

		public EpsgCoordOpMethodInfo OperationMethodInfo { get { return EpsgCoordOpMethodInfo.Get(_opMethodCode); } }

		public EpsgArea Area { get { return EpsgArea.Get(_areaCode); } }

		public string Name { get { return _name; } }

		public bool Deprecated { get { return _deprecated; } }

		public IEnumerable<INamedParameter> Parameters {
			get {
				var opMethodInfo = OperationMethodInfo;
				return null != opMethodInfo ? opMethodInfo.GetOperationParameters(_code) : null;
			}
		}

	}

}
