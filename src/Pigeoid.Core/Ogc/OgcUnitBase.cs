// TODO: source header

using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// The base OGC unit of measure class. All units are relative to a single base unit by some factor.
	/// </summary>
	public abstract class OgcUnitBase : OgcNamedAuthorityBoundEntity, IUom
	{
		private readonly double _factor;

		protected OgcUnitBase(string name, double factor)
			: this(name, factor, null) { }

		protected OgcUnitBase(string name, double factor, IAuthorityTag authority)
			: base(name, authority) {
			_factor = factor;
		}

		public abstract string Type { get; }

		string IUom.Name {
			get { return Name; }
		}

		/// <summary>
		/// The conversion factor for the reference unit.
		/// </summary>
		public double Factor {
			get { return _factor; }
		}

	}
}
