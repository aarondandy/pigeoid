// TODO: source header

using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A vertical CRS.
	/// </summary>
	public class OgcCrsVertical : OgcNamedAuthorityBoundEntity, ICrsVertical
	{

		private readonly IDatum _datum;
		private readonly IUom _unit;
		private readonly IAxis _axis;

		/// <summary>
		/// Constructs a new vertical CRS.
		/// </summary>
		/// <param name="name">The name of the CRS.</param>
		/// <param name="datum">The datum the CRS is based on.</param>
		/// <param name="linearUnit">The linear unit for the CRS.</param>
		/// <param name="axis">The axis for the linear CRS.</param>
		/// <param name="authority">The authority.</param>
		public OgcCrsVertical(
			string name,
			IDatum datum,
			IUom linearUnit,
			IAxis axis,
			IAuthorityTag authority
		) : base(name, authority) {
			_datum = datum;
			_unit = linearUnit;
			_axis = axis;
		}

		public IDatum Datum {
			get { return _datum; }
		}

		public IUom Unit {
			get { return _unit; }
		}

		public IAxis Axis {
			get { return _axis; }
		}
	}
}
