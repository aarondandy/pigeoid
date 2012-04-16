using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A fitted coordinate reference system.
	/// </summary>
	public interface ICrsFitted : ICrs
	{
		/// <summary>
		/// The base CRS of this fitted CRS.
		/// </summary>
		ICrs BaseCrs { get; }
		/// <summary>
		/// The operation which converts from this CRS to the base CRS.
		/// </summary>
		ITransformation ToBaseOperation { get; }
	}
}
