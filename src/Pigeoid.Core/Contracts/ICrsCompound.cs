// TODO: source header

using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A compound coordinate reference system composed of multiple other coordinate reference systems.
	/// </summary>
	public interface ICrsCompound : ICrs
	{

		/// <summary>
		/// The component coordinate reference systems that make up this compound coordinate reference system.
		/// </summary>
		IEnumerable<ICrs> CrsComponents { get; }

	}
}
