// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A local CRS.
	/// </summary>
	public class OgcCrsLocal : OgcNamedAuthorityBoundEntity, ICrsLocal
	{

		private readonly IDatum _datum;
		private readonly IUnit _unit;
		private readonly IList<IAxis> _axes;

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
			IUnit unit,
			[CanBeNull] IEnumerable<IAxis> axes,
			IAuthorityTag authority
		) : base(name, authority) {
			_datum = datum;
			_unit = unit;
			_axes = Array.AsReadOnly(null == axes ? new IAxis[0] : axes.ToArray());
		}

		/// <inheritdoc/>
		public IDatum Datum { get { return _datum; } }

		/// <inheritdoc/>
		public IUnit Unit { get { return _unit; } }

		/// <inheritdoc/>
		public IList<IAxis> Axes { get { return _axes; } }

	}
}
