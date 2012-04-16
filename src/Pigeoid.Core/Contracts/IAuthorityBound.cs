
using System;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// Marks an object as being bound to an authority source.
	/// </summary>
	[Obsolete("Replace with some kind of WKT converter info class: info.AuthorityResolver = obj => obj.Authority")]
	public interface IAuthorityBound
	{
		/// <summary>
		/// The authority tag.
		/// </summary>
		IAuthorityTag Authority { get; }
	}
}
