// TODO: source header

using System;
using System.Globalization;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	/// <summary>
	/// An EPSG authority tag.
	/// </summary>
	public struct EpsgAuthorityTag :
		IAuthorityTag,
		IEquatable<EpsgAuthorityTag>
	{

		public static bool operator==(EpsgAuthorityTag a, EpsgAuthorityTag b) {
			return a.Equals(b);
		}

		public static bool operator !=(EpsgAuthorityTag a, EpsgAuthorityTag b) {
			return !a.Equals(b);
		}

		internal const string EpsgName = "EPSG";

		private readonly int _code;

		internal EpsgAuthorityTag(int code) {
			_code = code;
		}

		public string Code {
			get { return _code.ToString(CultureInfo.InvariantCulture); }
		}

		public string Name {
			get { return EpsgName; }
		}

		public bool Equals(EpsgAuthorityTag other) {
			return other._code == _code;
		}

		public bool Equals(IAuthorityTag other) {
			return null != other
				&& Name.Equals(other.Name)
				&& Code.Equals(other.Code);
		}

		public override bool Equals(object obj) {
			return obj is EpsgAuthorityTag
				? Equals((EpsgAuthorityTag) obj)
				: Equals(obj as IAuthorityTag);
		}

		public override int GetHashCode() {
			return _code;
		}

		public override string ToString() {
			return Name + ':' + _code;
		}

	}
}
