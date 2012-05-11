// TODO: source header

using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	/// <summary>
	/// An EPSG authority tag.
	/// </summary>
	public struct EpsgAuthorityTag : IAuthorityTag
	{

		internal const string EpsgName = "EPSG";

		private readonly int _code;

		internal EpsgAuthorityTag(int code) {
			_code = code;
		}

		public string Code {
			get { return _code.ToString(); }
		}

		public string Name {
			get { return EpsgName; }
		}
	}
}
