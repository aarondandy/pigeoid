// TODO: source header

using Pigeoid.Transformation;

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
		ISpheroidInfo Spheroid { get; }
		/// <summary>
		/// The prime meridian.
		/// </summary>
		IPrimeMeridianInfo PrimeMeridian { get; }

		Helmert7Transformation BasicWgs84Transformation { get; }

		bool IsTransformableToWgs84 { get; }
	}

}
