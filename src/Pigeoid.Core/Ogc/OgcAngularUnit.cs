// TODO: source header

using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// An angular unit of measure.
	/// </summary>
	public class OgcAngularUnit : OgcUnitBase
	{

		private static readonly OgcAngularUnit _defaultRadian = new OgcAngularUnit("Radian", 1);
		// TODO: possibly calculate the degree conversion factor from a conversion graph?
		private static readonly OgcAngularUnit _defaultDegrees = new OgcAngularUnit("Degree", System.Math.PI / 180.0);

		/// <summary>
		/// This is the OGC reference unit for angular measure.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcAngularUnit DefaultRadians { get { return _defaultRadian; } }
		/// <summary>
		/// The default degree unit factored against radians.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcAngularUnit DefaultDegrees { get { return _defaultDegrees; } }

		/// <summary>
		/// Constructs a new unit.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="factor">The conversion factor to the base unit.</param>
		public OgcAngularUnit(string name, double factor)
			: base(name, factor) { }

		/// <summary>
		/// Constructs a new unit.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="factor">The conversion factor to the base unit.</param>
		/// <param name="authority">The authority.</param>
		public OgcAngularUnit(string name, double factor, IAuthorityTag authority)
			: base(name, factor, authority) { }

		public override string Type {
			get { return "angle"; }
		}
	}
}
