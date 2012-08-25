using System.Collections.Generic;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public static class NumberUtils
	{

		public static Dictionary<double, int> BuildNumberCountLookUp(IEnumerable<double> numberList) {
			var numberCounts = new Dictionary<double, int>();
			foreach (var number in numberList) {
				int currentCount;
				if (numberCounts.TryGetValue(number, out currentCount)) {
					numberCounts[number] = currentCount + 1;
				}
				else {
					numberCounts[number] = 1;
				}
			}
			return numberCounts;
		}

	}
}
