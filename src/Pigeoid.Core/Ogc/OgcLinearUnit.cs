// TODO: source header

using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A linear unit of measure.
	/// </summary>
	public class OgcLinearUnit : OgcUnitBase
	{

		private static readonly OgcLinearUnit DefaultMeterUnit = new OgcLinearUnit("meter", 1);

		/// <summary>
		/// The default OGC reference unit for length measures.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcLinearUnit DefaultMeter { get { return DefaultMeterUnit; } }

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
