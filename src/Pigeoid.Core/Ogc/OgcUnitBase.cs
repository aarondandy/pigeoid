// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// The base OGC unit of measure class. All units are relative to a single base unit by some factor.
	/// </summary>
	public abstract class OgcUnitBase : OgcNamedAuthorityBoundEntity, IUnit
	{
		private readonly double _factor;

		protected OgcUnitBase(string name, double factor)
			: this(name, factor, null) { }

		protected OgcUnitBase(string name, double factor, IAuthorityTag authority)
			: base(name, authority) {
			_factor = factor;
		}

		public abstract string Type { get; }

		string IUnit.Name {
			get { return Name; }
		}

		/// <summary>
		/// The conversion factor for the reference unit.
		/// </summary>
		public double Factor {
			get { return _factor; }
		}

		public IUnitConversionMap<double> ConversionMap {
			get {
				throw new NotImplementedException();
			}
		}

		// public abstract IEnumerable<IUnit> DirectlyConvertibleTo { get; }

		// public abstract IEnumerable<IUnit> DirectlyConvertibleFrom { get; }

		/*public IUnitConversion<double> GetDirectConversionTo(IUnit unit) {
			if (null == unit)
				throw new ArgumentNullException("unit");

			if (DirectlyConvertibleTo.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, unit.Name))) {
				// ReSharper disable CompareOfFloatsByEqualityOperator
				if(_factor == 1.0)
					return new UnitUnityConversion(this, unit);
				return new UnitScalarConversion(this, unit, Factor);
				// ReSharper restore CompareOfFloatsByEqualityOperator
			}

			return null;
		}

		public IUnitConversion<double> GetDirectConversionFrom(IUnit unit) {
			var conversion = GetDirectConversionTo(unit);
			return null != conversion && conversion.HasInverse ? conversion.GetInverse() : null;
		}*/
	}
}
