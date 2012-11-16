// TODO: source header

using System;
using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// An angular unit of measure.
	/// </summary>
	public class OgcAngularUnit : OgcUnitBase
	{

		private static readonly OgcAngularUnit DefaultRadianInstance = new OgcAngularUnit("Radian", 1);
		private static readonly OgcAngularUnit DefaultDegreesInstance = new OgcAngularUnit("Degree", Math.PI / 180.0, new AuthorityTag("EPSG", "9122"));

		/// <summary>
		/// This is the OGC reference unit for angular measure.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcAngularUnit DefaultRadians { get { return DefaultRadianInstance; } }
		/// <summary>
		/// The default degree unit factored against radians.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcAngularUnit DefaultDegrees { get { return DefaultDegreesInstance; } }

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

		public override string Type { get { return "angle"; } }

		public override IUnit ReferenceUnit {
			get { return DefaultRadians; }
		}

	}
}
