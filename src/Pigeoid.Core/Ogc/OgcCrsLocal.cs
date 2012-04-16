using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A local CRS.
	/// </summary>
	public class OgcCrsLocal :
		OgcNamedAuthorityBoundEntity,
		ICrsLocal
	{

		private readonly IDatum _datum;
		private readonly IUom _unit;
		private readonly IAxis[] _axes;

		/// <summary>
		/// Constructs a new local CRS.
		/// </summary>
		/// <param name="name">The CRS name.</param>
		/// <param name="datum">The datum the CRS is based on.</param>
		/// <param name="unit">The unit for the CRS.</param>
		/// <param name="axes">The axes of the CRS.</param>
		/// <param name="authority">The authority.</param>
		public OgcCrsLocal(
			string name,
			IDatum datum,
			IUom unit,
			IEnumerable<IAxis> axes,
			IAuthorityTag authority
		) : base(name, authority) {
			_datum = datum;
			_unit = unit;
			_axes = axes ==  null ? new IAxis[0] : axes.ToArray();
		}

		public IDatum Datum { get { return _datum; } }

		public IUom Unit { get { return _unit; } }

		public IEnumerable<IAxis> Axes { get { return _axes.AsEnumerable(); } }

	}
}
