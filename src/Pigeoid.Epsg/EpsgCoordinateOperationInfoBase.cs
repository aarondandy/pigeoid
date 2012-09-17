using System;
using System.Collections.Generic;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	public abstract class EpsgCoordinateOperationInfoBase : ICoordinateOperationInfo
	{

		private readonly ushort _code;
		private readonly ushort _areaCode;
		private readonly string _name;
		private readonly bool _deprecated;

		internal EpsgCoordinateOperationInfoBase(ushort code, ushort areaCode, bool deprecated, string name) {
			_code = code;
			_areaCode = areaCode;
			_deprecated = deprecated;
			_name = name;
		}

		public int Code { get { return _code; } }
		public string Name { get { return _name; } }
		public EpsgArea Area { get { return EpsgArea.Get(_areaCode); } }
		public bool Deprecated { get { return _deprecated; } }

		public abstract IEnumerable<INamedParameter> Parameters { get; }


		public abstract bool HasInverse { get; }

		public ICoordinateOperationInfo GetInverse() {
			if(HasInverse)
				return new EpsgCoordinateOperationInverse(this);

			throw new InvalidOperationException("Operation does not have an inverse.");
		}

		public bool IsInverseOfDefinition {
			get { return false; }
		}

	}

}
