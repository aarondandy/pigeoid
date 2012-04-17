using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A horizontal or geodetic datum.
	/// </summary>
	public class OgcDatumHorizontal :
		OgcDatum,
		IDatumGeodetic,
		ITransformableToWgs84
	{
		private readonly ISpheroid<double> _spheroid;
		private readonly IPrimeMeridian _primeMeritidan;
		private readonly Helmert7Transformation _transformation;

		/// <summary>
		/// Constructs a horizontal datum.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="spheroid">The spheroid of hte datum.</param>
		/// <param name="primeMeridian">The prime meridian of the datum.</param>
		/// <param name="transform">The transformation for conversions to WGS84.</param>
		/// <param name="authority">The authority.</param>
		public OgcDatumHorizontal(
			string name,
			ISpheroid<double> spheroid,
			IPrimeMeridian primeMeridian,
			Helmert7Transformation transform,
			IAuthorityTag authority
		) : base(name, OgcDatumType.None, authority) {
			if (null == spheroid)
				throw new ArgumentNullException("spheroid");

			_primeMeritidan = primeMeridian;
			_transformation = transform;
			_spheroid = spheroid;
		}

		public bool IsTransformableToWgs84 {
			get { return null != _transformation; }
		}

		public Helmert7Transformation PrimaryTransformation {
			get { return _transformation; }
		}

		public IEnumerator<Helmert7Transformation> GetEnumerator() {
			return new[] { _transformation }.AsEnumerable().GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IPrimeMeridian PrimeMeridian {
			get { return _primeMeritidan; }
		}

		public ISpheroid<double> Spheroid {
			get { return _spheroid; }
		}

	}
}
