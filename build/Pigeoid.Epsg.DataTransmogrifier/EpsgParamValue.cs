using System;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgParamValue : IEquatable<EpsgParamValue>
	{

		public virtual EpsgCoordinateOperation Operation { get; set; }

		public virtual EpsgCoordinateOperationMethod Method { get; set; }

		public virtual EpsgParameter Parameter { get; set; }

		public virtual double? NumericValue { get; set; }

		public virtual string TextValue { get; set; }

		public virtual EpsgUom Uom { get; set; }

		public override string ToString() {
			return Operation.ToString() + ',' + Method.ToString() + ',' + Parameter.ToString();
		}

		public override bool Equals(object obj) {
			return Equals(obj as EpsgParamUse);
		}

		public virtual bool Equals(EpsgParamValue other) {
			if (ReferenceEquals(this, other))
				return true;
			if (ReferenceEquals(null, other))
				return false;

			var operationOk = Operation == other.Operation || (
				null == Operation
				? null == other.Operation
				: null != other.Operation && Operation.Code == other.Operation.Code
			);
			if (!operationOk)
				return false;

			var methodOk = Method == other.Method || (
				null == Method
				? null == other.Method
				: null != other.Method && Method.Code == other.Method.Code
			);
			if (!methodOk)
				return false;

			var paramOk = Parameter == other.Parameter || (
				null == Parameter
				? null == other.Parameter
				: null != other.Parameter && Parameter.Code == other.Parameter.Code
			);
			if (!paramOk)
				return false;

			return true;
		}

		public override int GetHashCode() {
			var a = null == Operation ? 0 : Operation.Code;
			var b = null == Parameter ? 0 : Parameter.Code;
			return a ^ -b;
		}

	}
}
