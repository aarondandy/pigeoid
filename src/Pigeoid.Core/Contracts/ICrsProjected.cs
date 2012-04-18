// TODO: source header

using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A projected coordinate reference system.
	/// </summary>
	public interface ICrsProjected : ICrsGeodetic
	{
		/// <summary>
		/// The CRS this projected CRS is based on.
		/// </summary>
		ICrsGeodetic BaseCrs { get; }
		/// <summary>
		/// The projection operation.
		/// </summary>
		ITransformation Projection { get; }

	}
}
