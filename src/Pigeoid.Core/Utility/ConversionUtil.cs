using System;
using System.ComponentModel;
using System.Globalization;

namespace Pigeoid.Utility
{
    internal static class ConversionUtil
    {

        public static bool TryParseDoubleMultiCulture(string text, out double value) {
            return Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value)
                || Double.TryParse(text, out value);
        }

        public static bool TryConvertDoubleMultiCulture(object rawValue, out double value) {
            if (rawValue == null) {
                value = default(double);
                return false;
            }

            var valueType = rawValue.GetType();
            if (valueType == typeof(double)) {
                value = (double)rawValue;
                return true;
            }
            if (valueType == typeof(int)) {
                value = (int)rawValue;
                return true;
            }
            if (valueType == typeof(uint)) {
                value = (uint)rawValue;
                return true;
            }

            if (rawValue == typeof(string))
                return TryParseDoubleMultiCulture((string)rawValue, out value);

            var converter = TypeDescriptor.GetConverter(valueType);
            if (converter.CanConvertTo(typeof(double))) {
                value = (double)converter.ConvertTo(rawValue, typeof(double));
                return true;
            }

            value = default(double);
            return false;
        }

    }
}
