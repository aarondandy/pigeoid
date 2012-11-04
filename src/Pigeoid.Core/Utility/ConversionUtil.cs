using System;
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
			if(rawValue is double) {
				value = (double)rawValue;
				return true;
			}
			if(rawValue is int) {
				value = (int)rawValue;
				return true;
			}
			if(rawValue is uint) {
				value = (uint)rawValue;
				return true;
			}
			if(rawValue is string) {
				return TryParseDoubleMultiCulture((string)rawValue, out value);
			}
			try {
				value = Convert.ToDouble(rawValue);
				return true;
			}
			catch {
				value = default(double);
				return false;
			}
		}

	}
}
