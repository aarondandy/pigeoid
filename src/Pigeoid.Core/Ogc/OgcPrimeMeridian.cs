using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A prime meridian.
	/// </summary>
	public class OgcPrimeMeridian :
		OgcNamedAuthorityBoundEntity,
		IPrimeMeridian
	{
		private readonly double _longitude;
		private readonly IUom _unit;

		/// <summary>
		/// Constructs a prime meridian.
		/// </summary>
		/// <param name="name">The name of the prime meridian.</param>
		/// <param name="longitude">The longitude location of the meridian.</param>
		/// <param name="authority">The authority.</param>
		public OgcPrimeMeridian(string name, double longitude, IAuthorityTag authority)
			: this(name, longitude, null, authority) { }

		/// <summary>
		/// Constructs a prime meridian.
		/// </summary>
		/// <param name="name">The name of the prime meridian.</param>
		/// <param name="longitude">The longitude location of the meridian.</param>
		/// <param name="angularUnit">The angular unit of the longitude value.</param>
		/// <param name="authority">The authority.</param>
		public OgcPrimeMeridian(string name, double longitude, IUom angularUnit, IAuthorityTag authority)
			: base(name, authority) {
			_longitude = longitude;
			_unit = angularUnit; // TODO: default to degrees if no unit is specified
		}

		public double Longitude {
			get { return _longitude; }
		}

		public IUom Unit {
			get { return _unit; }
		}

	}
}
