// TODO: source header

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A geographic coordinate.
	/// </summary>
	/// <typeparam name="TValue">The element type.</typeparam>
	public interface IGeographicCoord<TValue>
	{

		/// <summary>
		/// The latitude component of the coordinate.
		/// </summary>
		double Latitude { get; }
		/// <summary>
		/// The longitude component of the coordinate.
		/// </summary>
		double Longitude { get; }

	}
}
