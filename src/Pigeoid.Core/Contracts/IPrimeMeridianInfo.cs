// TODO: source header

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A prime meridian.
	/// </summary>
	public interface IPrimeMeridianInfo : IAuthorityBoundEntity
	{
		/// <summary>
		/// The longitude of the prime meridian.
		/// </summary>
		double Longitude { get; }
		/// <summary>
		/// The unit of measure for the longitude.
		/// </summary>
		IUnit Unit { get; }

		string Name { get; }

	}
	
}
