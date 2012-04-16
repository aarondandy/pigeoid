using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;

namespace Pigeoid
{
	/// <summary>
	/// An authority tag used to identify an object.
	/// </summary>
	public class AuthorityTag :
		IAuthorityTag
	{
		/// <summary>
		/// The authority name.
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// The authority code.
		/// </summary>
		public readonly string Code;

		/// <summary>
		/// Constructs a new authority tag.
		/// </summary>
		/// <param name="name">The authority name.</param>
		/// <param name="code">The authority code.</param>
		public AuthorityTag(string name, string code) {
			Name = name;
			Code = code;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string IAuthorityTag.Name {
			get { return Name; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string IAuthorityTag.Code {
			get { return Code; }
		}
	}
}
