using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Transformation;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A geographic CRS.
	/// </summary>
	public class OgcCrsGeographic :
		OgcNamedAuthorityBoundEntity,
		ICrsGeographic,
		ITransformableToWgs84
	{

		private readonly Helmert7Transformation[] _toWgs84;
		private readonly IUom _unit;
		private readonly IAxis[] _axes;
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
			IAuthorityTag authority
		) : this(name, datum, angularUnit, axes, null, authority) { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">The name of the CRS.</param>
		/// <param name="datum">The datum the CRS is based on.</param>
		/// <param name="angularUnit">The angular unit of measure for the CRS.</param>
		/// <param name="axes">The axes defining the space.</param>
		/// <param name="toWgs84">The transform which converts this CRS to a standard WGS84 CRS.</param>
		/// <param name="authority">The authority.</param>
		public OgcCrsGeographic(
			string name,
			IDatumGeodetic datum,
			IUom angularUnit,
			IEnumerable<IAxis> axes,
			IEnumerable<Helmert7Transformation> toWgs84,
			IAuthorityTag authority
		) : base(name, authority) {
			_datum = datum;
			_unit = angularUnit;
			_axes = null == axes ? new IAxis[0] : axes.ToArray();
			_toWgs84 = null == toWgs84 ? new Helmert7Transformation[0] : toWgs84.ToArray();
		}

		public IDatumGeodetic Datum { get { return _datum; } }

		public IUom Unit { get { return _unit; } }

		public IEnumerable<IAxis> Axes { get { return _axes.AsEnumerable(); } }

		public bool IsTransformableToWgs84 { get { return 0 != _toWgs84.Length; } }

		public Helmert7Transformation PrimaryWgs84Transformation {
			get { return (0 < _toWgs84.Length) ? _toWgs84[0] : null; }
		}

		public IEnumerable<Helmert7Transformation> Wgs84Transformations {
			get { return _toWgs84.AsEnumerable(); }
		}
	}
}
