// TODO: source header

using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid
{
	/// <summary>
	/// An authority tag used to identify an object.
	/// </summary>
	public class AuthorityTag : IAuthorityTag
	{
		/// <summary>
		/// The authority name.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string _name;

		/// <summary>
		/// The authority code.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string _code;

		/// <summary>
		/// Constructs a new authority tag.
		/// </summary>
		/// <param name="name">The authority name.</param>
		/// <param name="code">The authority code.</param>
		public AuthorityTag(string name, string code) {
			_name = name;
			_code = code;
		}

		/// <inheritdoc/>
		public string Name {
			get { return _name; }
		}

		/// <inheritdoc/>
		public string Code {
			get { return _code; }
		}

		public bool Equals(IAuthorityTag other) {
			return null != other
				&& Name.Equals(other.Name)
				&& Code.Equals(other.Code);
		}

		public override bool Equals(object obj) {
			return ReferenceEquals(this,obj) || Equals(obj as IAuthorityTag);
		}

		public override int GetHashCode() {
			return Code.GetHashCode();
		}

		public override string ToString() {
			return Name + ':' + Code;
		}

	}
}
