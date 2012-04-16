using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A compound coordinate reference system composed of multiple other coordinate reference systems.
	/// </summary>
	public interface ICrsCompound : ICrs, IEnumerable<ICrs>
	{
	}
}
