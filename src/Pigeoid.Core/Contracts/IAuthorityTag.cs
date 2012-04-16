using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// Definition of an authority tag complete with an authority name and item code.
	/// </summary>
	public interface IAuthorityTag
	{

		/// <summary>
		/// The name of the authority.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The authority tag code.
		/// </summary>
		string Code { get; }
	}
}
