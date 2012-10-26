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

		public abstract IEnumerable<IUom> ConvertibleTo { get; }

		public abstract IEnumerable<IUom> ConvertibleFrom { get; }

		public IUomConversion<double> GetConversionTo(IUom uom) {
			if (null == uom)
				throw new ArgumentNullException("uom");

			if (ConvertibleTo.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, uom.Name))) {
// ReSharper disable CompareOfFloatsByEqualityOperator
				if(_factor == 1.0)
					return new UomUnityConversion(this, uom);
				return new UomScalarConversion(this, uom, Factor);
				// ReSharper restore CompareOfFloatsByEqualityOperator
			}

			return null;
		}

		public IUomConversion<double> GetConversionFrom(IUom uom) {
			var conversion = GetConversionTo(uom);
			return null != conversion && conversion.HasInverse ? conversion.GetInverse() : null;
		}
	}
}
