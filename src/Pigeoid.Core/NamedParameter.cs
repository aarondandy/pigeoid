// TODO: source header

using System;
using System.Diagnostics;
using System.Globalization;
using Pigeoid.Contracts;
using JetBrains.Annotations;
using Pigeoid.Utility;

namespace Pigeoid
{
	/// <summary>
	/// A named parameter.
	/// </summary>
	/// <typeparam name="TValue">The value type of the parameter.</typeparam>
	public class NamedParameter<TValue> : INamedParameter
	{

		/// <summary>
		/// The parameter name.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string _name;

		/// <summary>
		/// The parameter value.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly TValue _value;

		/// <summary>
		/// The optional unit of measure for the parameter value.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IUnit _unit;

		public NamedParameter(string name, TValue value)
			: this(name, value, null) { }

		public NamedParameter(string name, TValue value, IUnit unit) {
			_name = name;
			_value = value;
			_unit = unit;
		}

		/// <summary>
		/// Parameter name.
		/// </summary>
		public string Name { get { return _name; } }

		/// <summary>
		/// Parameter value.
		/// </summary>
		public TValue Value { get { return _value; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object INamedParameter.Value { get { return _value; } }

		public IUnit Unit { get { return _unit; } }

		[ContractAnnotation("=>notnull")]
		private string GetValueUnitString(){
			if (null == Value)
				return String.Empty;
			var result = Value.ToString();
			if (null != Unit)
				result += Unit.ToString();
			return result;
		}

		[ContractAnnotation("=>notnull")]
		public override string ToString(){
			return null == Name
				? (null == Value ? base.ToString() : GetValueUnitString())
				: (null == Value ? Name : String.Concat(Name, ": ", GetValueUnitString()));
		}
	}

	public static class NamedParameter
	{
		[ContractAnnotation("parameter:null=>false")]
		public static bool TryGetDouble([CanBeNull] INamedParameter parameter, out double value) {
			if (null != parameter) {
				var doubleParameter = parameter as NamedParameter<double>;
				if (null != doubleParameter) {
					value = doubleParameter.Value;
					return true;
				}
				if (parameter is NamedParameter<string>) {
					return ConversionUtil.TryParseDoubleMultiCulture(((NamedParameter<string>)parameter).Value, out value);
				}
				var rawValue = parameter.Value;
				if(rawValue is double) {
					value = (double)rawValue;
					return true;
				}
				if (rawValue is string) {
					return ConversionUtil.TryParseDoubleMultiCulture((string)rawValue, out value);
				}
				return ConversionUtil.TryConvertDoubleMultiCulture(rawValue, out value);
			}
			value = default(double);
			return false;
		}

		

	}

}
