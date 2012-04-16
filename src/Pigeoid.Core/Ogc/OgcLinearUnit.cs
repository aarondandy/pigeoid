using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A linear unit of measure.
	/// </summary>
	public class OgcLinearUnit : OgcUnitBase
	{
		/// <summary>
		/// Constructs a new unit.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="factor">The conversion factor to the base unit.</param>
		public OgcLinearUnit(string name, double factor)
			: base(name, factor) { }

		/// <summary>
		/// Constructs a new unit.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="factor">The conversion factor to the base unit.</param>
		/// <param name="authority">The authority.</param>
		public OgcLinearUnit(string name, double factor, IAuthorityTag authority)
			: base(name, factor, authority) { }

		public override string Type {
			get { return "length"; }
		}
	}
}
