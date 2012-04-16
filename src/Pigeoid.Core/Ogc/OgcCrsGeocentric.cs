using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A geocentric CRS.
	/// </summary>
	public class OgcCrsGeocentric : OgcNamedAuthorityBoundEntity, ICrsGeocentric
	{

		private readonly IDatumGeodetic _datum;
		private readonly IUom _unit;
		private readonly IAxis[] _axes;

		/// <summary>
		/// Constructs a new geocentric CRS.
		/// </summary>
		/// <param name="name">The name of the CRS.</param>
		/// <param name="datum">The datum the CRS is based on.</param>
		/// <param name="linearUnit">The linear UoM to use for the CRS.</param>
		/// <param name="axes">The axes which define the space.</param>
		/// <param name="authority">The authority.</param>
		public OgcCrsGeocentric(
			string name,
			IDatumGeodetic datum,
			IUom linearUnit,
			IEnumerable<IAxis> axes,
			IAuthorityTag authority
		) : base(name, authority) {
			if (null == datum)
				throw new ArgumentNullException("datum");
			if (null == linearUnit)
				throw new ArgumentNullException("linearUnit");

			_datum = datum;
			_unit = linearUnit;
			_axes = (null == axes) ? new IAxis[0] : axes.ToArray();
		}

		public IDatumGeodetic Datum { get { return _datum; } }

		public IUom Unit { get { return _unit; } }

		public IEnumerable<IAxis> Axes{ get { return _axes.AsEnumerable(); } }

	}
}
