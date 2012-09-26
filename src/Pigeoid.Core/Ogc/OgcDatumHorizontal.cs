// TODO: source header

using System;
using Pigeoid.Contracts;
using Pigeoid.Transformation;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A horizontal or geodetic datum.
	/// </summary>
	public class OgcDatumHorizontal : OgcDatum, IDatumGeodetic
	{
		private readonly ISpheroidInfo _spheroid;
		private readonly IPrimeMeridianInfo _primeMeridian;
		private readonly Helmert7Transformation _transformation;

		/// <summary>
		/// Constructs a horizontal datum.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="spheroid">The spheroid of the datum.</param>
		/// <param name="primeMeridian">The prime meridian of the datum.</param>
		/// <param name="transform">The transformation for conversions to WGS84.</param>
		/// <param name="authority">The authority.</param>
		public OgcDatumHorizontal(
			string name,
			ISpheroidInfo spheroid,
			IPrimeMeridianInfo primeMeridian,
			Helmert7Transformation transform,
			IAuthorityTag authority = null
		)
			: base(name, OgcDatumType.None, authority)
		{
			if (null == spheroid)
				throw new ArgumentNullException("spheroid");

			_primeMeridian = primeMeridian;
			_transformation = transform;
			_spheroid = spheroid;
		}

		public IPrimeMeridianInfo PrimeMeridian {
			get { return _primeMeridian; }
		}

		public ISpheroidInfo Spheroid {
			get { return _spheroid; }
		}

		public bool IsTransformableToWgs84 {
			get { return null != _transformation; }
		}

		public Helmert7Transformation BasicWgs84Transformation { get { return _transformation; } }
	}
}
