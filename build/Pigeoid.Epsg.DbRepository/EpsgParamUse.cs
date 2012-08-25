using System;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgParamUse : IEquatable<EpsgParamUse>
	{

		public virtual EpsgCoordinateOperationMethod Method { get; set; }

		public virtual EpsgParameter Parameter { get; set; }

		public virtual int SortOrder { get; set; }

		public virtual string SignReversalText { get; set; }

		public virtual bool? SignReversal {
			get {
				return String.IsNullOrWhiteSpace(SignReversalText)
					? (bool?)null
					: String.Equals("YES", SignReversalText, StringComparison.OrdinalIgnoreCase);
			}
		}

		public override string ToString() {
			return String.Concat(Method,',',Parameter);
		}

		public override bool Equals(object obj) {
			return Equals(obj as EpsgParamUse);
		}

		public virtual bool Equals(EpsgParamUse other) {
			if (ReferenceEquals(this, other))
				return true;
			if (ReferenceEquals(null, other))
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
			var a = null == Method ? 0 : Method.Code;
			var b = null == Parameter ? 0 : Parameter.Code;
			return a ^ -b;
		}
	}
}
