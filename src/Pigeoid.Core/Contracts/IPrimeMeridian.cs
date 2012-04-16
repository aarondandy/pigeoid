
namespace Pigeoid.Contracts
{
	/// <summary>
	/// A prime meridian.
	/// </summary>
	public interface IPrimeMeridian
	{
		/// <summary>
		/// The longitude of the prime meridian.
		/// </summary>
		double Longitude { get; }
		/// <summary>
		/// The unit of measure for the longitude.
		/// </summary>
		IUom Unit { get; }
	}
	
}
