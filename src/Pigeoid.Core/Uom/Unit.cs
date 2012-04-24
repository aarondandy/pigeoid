// TODO: source header

using System;
using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid.Uom
{
	/// <summary>
	/// A unit of measure.
	/// </summary>
	public class Unit :
		IUom
	{
		/// <summary>
		/// The name.
		/// </summary>
		private readonly string _name;

		/// <summary>
		/// The category the unit of measure belongs to.
		/// </summary>
		private readonly string _type;

		/// <summary>
		/// Constructs a unit of measure with the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="authority">The authority.</param>
		public Unit(string name, string type) {
			if (String.IsNullOrEmpty(name))
				throw new ArgumentException("Invalid unit name.","name");
			if (String.IsNullOrEmpty(type))
				throw new ArgumentException("Invalid type name.","type");
			_name = name;
			_type = type;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public string Name {
			get { return _name; }
		}

		/// <inheritdoc/>
		public override string ToString() {
			return _type + ':' + _name;
		}

		/// <inheritdoc/>
		public string Type {
			get { return _type; }
		}
	}
}
