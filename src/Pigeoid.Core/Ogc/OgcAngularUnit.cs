using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// An angular unit of measure.
	/// </summary>
	public class OgcAngularUnit : OgcUnitBase
	{
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
