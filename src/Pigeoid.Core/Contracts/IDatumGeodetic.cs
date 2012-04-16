using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{
	
	/// <summary>
	/// A geodetic datum.
	/// </summary>
	public interface IDatumGeodetic : IDatum
	{
		/// <summary>
		/// The spheroid.
		/// </summary>
		ISpheroid<double> Spheroid { get; }
		/// <summary>
		/// The prime meridian.
		/// </summary>
		IPrimeMeridian PrimeMeridian { get; }
	}

}
