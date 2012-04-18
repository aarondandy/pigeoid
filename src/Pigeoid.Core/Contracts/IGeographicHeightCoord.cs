// TODO: source header

namespace Pigeoid.Contracts
{
	public interface IGeographicHeightCoord<TValue> : IGeographicCoord<TValue>
	{

		/// <summary>
		/// The height above the reference surface.
		/// </summary>
		double Height { get; }

	}
}
