// TODO: source header

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A compound coordinate reference system composed of multiple other coordinate reference systems.
	/// </summary>
	public interface ICrsCompound : ICrs
	{

		ICrs Head { get; }

		ICrs Tail { get; }

	}
}
