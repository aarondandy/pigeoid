// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Interop.Proj4
{
	/// <summary>
	/// A datum used for Proj4 interop.
	/// </summary>
	public class Proj4Datum : IDatumGeodetic
	{

		private static readonly Proj4Datum[] Datums;

		static Proj4Datum() {
			Datums = new[] {
                new Proj4Datum("WGS84",Proj4Spheroid.GetSpheroid("WGS84"),Helmert7Transformation.IdentityTransformation),
                new Proj4Datum(
                    "GGRS87",Proj4Spheroid.GetSpheroid("GRS80"),
                    new Helmert7Transformation(new Vector3(-199.87,74.79,246.62))
                ), 
                new Proj4Datum(
                    "NAD83",
                    Proj4Spheroid.GetSpheroid("GRS80"),
                    Helmert7Transformation.IdentityTransformation,
                    matchExplicitly: true
                ), 
                new Proj4Datum(
                    "NAD27",
                    Proj4Spheroid.GetSpheroid("clrk66"),
                    null,
                    matchExplicitly: true
                ),
                new Proj4Datum(
                    "potsdam",Proj4Spheroid.GetSpheroid("bessel"),
                    new Helmert7Transformation(new Vector3(606.0,23.0,413.0))
                ),
                new Proj4Datum(
                    "carthage",Proj4Spheroid.GetSpheroid("clrk80"),
                    new Helmert7Transformation(new Vector3(-263.0,6.0,431.0))
                ),
                new Proj4Datum(
                    "hermannskogel",Proj4Spheroid.GetSpheroid("bessel"),
                    new Helmert7Transformation(new Vector3(653.0,-212.0,449.0))
                ),
                new Proj4Datum(
                    "ire65",Proj4Spheroid.GetSpheroid("mod_airy"),
                    new Helmert7Transformation(new Vector3(482.530,-130.596,564.557),new Vector3(-1.042,-0.214,-0.631),8.15)
                ),
                new Proj4Datum(
                    "nzgd49",Proj4Spheroid.GetSpheroid("intl"),
                    new Helmert7Transformation(new Vector3(59.47,-5.04,187.44),new Vector3(0.47,-0.1,1.024),-4.5993),
                    supported:false,
                    matchExplicitly:true
                ),
                new Proj4Datum(
                    "OSGB36",Proj4Spheroid.GetSpheroid("airy"),
                    new Helmert7Transformation(new Vector3(446.448,-125.157,542.060),new Vector3(0.1502,0.2470,0.8421),-20.4894),
                    supported:false
                ),
            };
		}

		/// <summary>
		/// All Proj4 datums.
		/// </summary>
		public static IEnumerable<Proj4Datum> AllDatums {
			get { return Datums.AsEnumerable(); }
		}

		private readonly string _name;
		private readonly Proj4Spheroid _spheroid;
		private readonly Helmert7Transformation _toWgs84;
		private readonly bool _supported;
		private readonly bool _matchExplicitly;

		/// <summary>
		/// Constructs a new Proj4 datum object.
		/// </summary>
		/// <param name="name">The Proj4 datum name.</param>
		/// <param name="ellipse">The ellipse for this datum.</param>
		/// <param name="toWgs84">A transformation to WGS84.</param>
		/// <param name="supported">Indicates if this datum is supported by Proj4.</param>
		/// <param name="matchExplicitly">Indicated is this datum must be matched explicitly by name only.</param>
		private Proj4Datum(
			string name,
			Proj4Spheroid spheroid,
			Helmert7Transformation toWgs84,
			bool supported = true,
			bool matchExplicitly = false
		) {
			_name = name;
			_spheroid = spheroid;
			_toWgs84 = toWgs84;
			_supported = supported;
			_matchExplicitly = matchExplicitly;
		}

		/// <summary>
		/// Indicates if this datum is supported by Proj4.
		/// </summary>
		public bool Supported { get { return _supported; } }

		/// <summary>
		/// Indicated is this datum must be matched explicitly by name only.
		/// </summary>
		/// <remarks>
		/// When false a Proj4 datum may also be matched based on spheroid parameters.
		/// </remarks>
		public bool MatchExplicitly { get { return _matchExplicitly; } }

		IPrimeMeridian IDatumGeodetic.PrimeMeridian {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// The spheroid for this datum.
		/// </summary>
		public Proj4Spheroid Spheroid {
			get { return _spheroid; }
		}

		ISpheroid<double> IDatumGeodetic.Spheroid {
			get { return _spheroid; }
		}

		public string Name {
			get { return _name; }
		}

		public bool IsTransformableToWgs84 {
			get { return null != _toWgs84; }
		}

		public Helmert7Transformation PrimaryWgs84Transformation {
			get { return _toWgs84; }
		}

	}
}
