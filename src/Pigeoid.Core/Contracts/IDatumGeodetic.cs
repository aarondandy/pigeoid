// TODO: source header

using Pigeoid.Transformation;
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
		IPrimeMeridianInfo PrimeMeridian { get; }

		Helmert7Transformation BasicWgs84Transformation { get; }

		bool IsTransformableToWgs84 { get; }
	}

}
