using System;
using System.ComponentModel;
using System.Globalization;

namespace Pigeoid.Utility
{
    internal static class ConversionUtil
    {

        public static bool? ConvertBooleanMultiCulture(object rawValue) {
            if (rawValue == null)
                return null;

            var valueType = rawValue.GetType();
            if (valueType == typeof(bool))
                return (bool)rawValue;

            bool value;
            if (valueType == typeof(string) && Boolean.TryParse((string)rawValue, out value))
                return value;

            var converter = TypeDescriptor.GetConverter(valueType);
            if (converter.CanConvertTo(typeof(bool)))
                return (bool)converter.ConvertTo(rawValue, typeof(bool));

            return null;
        }

        public static bool TryParseInt32MultiCulture(string text, out int value) {
            return Int32.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value)
                || Int32.TryParse(text, out value);
        }

        public static bool TryConvertInt32MultiCulture(object rawValue, out int value) {
            if (rawValue == null) {
                value = default(int);
                return false;
            }

            var valueType = rawValue.GetType();
            if (valueType == typeof(int)) {
                value = (int)rawValue;
                return true;
            }
            if (valueType == typeof(double)) {
                value = (int)(double)rawValue;
                return true;
            }

            if (valueType == typeof(string))
                return TryParseInt32MultiCulture((string)rawValue, out value);

            var converter = TypeDescriptor.GetConverter(valueType);
            if (converter.CanConvertTo(typeof(int))) {
                value = (int)converter.ConvertTo(rawValue, typeof(int));
                return true;
            }

            value = default(int);
            return false;
        }

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

            if (valueType == typeof(string))
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
