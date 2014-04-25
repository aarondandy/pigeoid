using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Pigeoid.Unit;
using Pigeoid.Utility;

namespace Pigeoid
{
    /// <summary>
    /// A named parameter.
    /// </summary>
    /// <typeparam name="TValue">The value type of the parameter.</typeparam>
    public class NamedParameter<TValue> : INamedParameter
    {

        public NamedParameter(string name, TValue value)
            : this(name, value, null) { }

        public NamedParameter(string name, TValue value, IUnit unit) {
            Name = name ?? String.Empty;
            Value = value;
            Unit = unit;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Name != null);
        }

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Parameter value.
        /// </summary>
        public TValue Value { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object INamedParameter.Value { get { return Value; } }

        /// <summary>
        /// The optional unit of measure for the parameter value.
        /// </summary>
        public IUnit Unit { get; private set; }

        private string GetValueUnitString() {
            Contract.Ensures(Contract.Result<string>() != null);
            if (null == Value)
                return String.Empty;
            var result = Value.ToString();
            if (null != Unit)
                result += Unit.ToString();
            return result;
        }

        public override string ToString() {
            Contract.Ensures(Contract.Result<string>() != null);
            return String.IsNullOrWhiteSpace(Name)
                ? (null == Value ? base.ToString() : GetValueUnitString())
                : (null == Value ? Name : String.Concat(Name, ": ", GetValueUnitString()));
        }
    }

    public static class NamedParameter
    {
        public static bool TryGetDouble(INamedParameter parameter, out double value) {
            Contract.Ensures(parameter == null ? !Contract.Result<bool>() : true);

            if (null == parameter) {
                value = default(double);
                return false;
            }

            var doubleParameter = parameter as NamedParameter<double>;
            if (null != doubleParameter) {
                value = doubleParameter.Value;
                return true;
            }
            if (parameter is NamedParameter<string>) {
                return ConversionUtil.TryParseDoubleMultiCulture(((NamedParameter<string>)parameter).Value, out value);
            }
            var rawValue = parameter.Value;
            if (rawValue is double) {
                value = (double)rawValue;
                return true;
            }
            if (rawValue is string) {
                return ConversionUtil.TryParseDoubleMultiCulture((string)rawValue, out value);
            }
            return ConversionUtil.TryConvertDoubleMultiCulture(rawValue, out value);
        }

    }

}
