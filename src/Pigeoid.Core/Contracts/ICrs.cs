// TODO: source header

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A coordinate reference system.
	/// </summary>
	public interface ICrs : IAuthorityBoundEntity
	{
		// TODO: maybe IDatum Datum { get; } ?

		// TODO: maybe IUom Unit { get; } ?

		string Name { get; }

	}

}
