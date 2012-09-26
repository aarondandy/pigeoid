// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A geographic CRS.
	/// </summary>
	public class OgcCrsGeographic : OgcNamedAuthorityBoundEntity, ICrsGeographic
	{

		private readonly IUom _unit;
		private readonly IList<IAxis> _axes;
		private readonly IDatumGeodetic _datum;

		/// <summary>
		/// Constructs a new geographic CRS.
		/// </summary>
		/// <param name="name">The name of the CRS.</param>
		/// <param name="datum">The datum the CRS is based on.</param>
		/// <param name="angularUnit">The angular unit of measure for the CRS.</param>
		/// <param name="axes">The axes defining the space.</param>
		/// <param name="authority">The authority.</param>
		public OgcCrsGeographic(
			string name,
			IDatumGeodetic datum,
			IUom angularUnit,
			IEnumerable<IAxis> axes,
			IAuthorityTag authority = null
		) : base(name, authority) {
			if (null == datum)
				throw new ArgumentNullException("datum");
			if (null == angularUnit)
				throw new ArgumentNullException("angularUnit");

			_datum = datum;
			_unit = angularUnit;
			_axes = Array.AsReadOnly(null == axes ? new IAxis[0] : axes.ToArray());
		}

		/// <inheritdoc/>
		public IDatumGeodetic Datum { get { return _datum; } }

		/// <inheritdoc/>
		public IUom Unit { get { return _unit; } }

		/// <inheritdoc/>
		public IList<IAxis> Axes { get { return _axes; } }

	}
}
