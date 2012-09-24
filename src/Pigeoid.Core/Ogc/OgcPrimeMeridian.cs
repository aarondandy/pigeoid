// TODO: source header

using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A prime meridian.
	/// </summary>
	public class OgcPrimeMeridian :
		OgcNamedAuthorityBoundEntity,
		IPrimeMeridianInfo
	{
		private readonly double _longitude;
		private readonly IUom _unit;

		/// <summary>
		/// Constructs a prime meridian.
		/// </summary>
		/// <param name="name">The name of the prime meridian.</param>
		/// <param name="longitude">The longitude location of the meridian.</param>
		/// <param name="authority">The authority.</param>
		public OgcPrimeMeridian(string name, double longitude, IAuthorityTag authority = null)
			: this(name, longitude, null, authority) { }

		/// <summary>
		/// Constructs a prime meridian.
		/// </summary>
		/// <param name="name">The name of the prime meridian.</param>
		/// <param name="longitude">The longitude location of the meridian.</param>
		/// <param name="angularUnit">The angular unit of the longitude value.</param>
		/// <param name="authority">The authority.</param>
		public OgcPrimeMeridian(string name, double longitude, IUom angularUnit, IAuthorityTag authority = null)
			: base(name, authority) {
			_longitude = longitude;
			_unit = angularUnit ?? OgcAngularUnit.DefaultDegrees;
		}

		public double Longitude {
			get { return _longitude; }
		}

		public IUom Unit {
			get { return _unit; }
		}

	}
}
