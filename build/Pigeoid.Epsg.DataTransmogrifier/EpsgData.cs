using System.Collections.Generic;
using System.IO;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgData
	{

		public readonly EpsgRepository Repository;

		public EpsgData(EpsgRepository repository) {
			Repository = repository;
		}

		public List<string> WordLookupList { get; set; }

		public void SetNumberLists(IEnumerable<double> numbers) {
			var dList = new List<double>();
			var iList = new List<double>();
			var sList = new List<double>();
			foreach (var n in numbers) {
				List<double> target;
				unchecked {
					if ((short)n == n) {
						target = sList;
					}
					else if ((int)n == n) {
						target = iList;
					}
					else {
						target = dList;
					}
				}
				target.Add(n);
			}
			NumberLookupDouble = dList;
			NumberLookupInt = iList;
			NumberLookupShort = sList;
		}

		public List<double> NumberLookupDouble { get; private set; }

		public List<double> NumberLookupInt { get; private set; }

		public List<double> NumberLookupShort { get; private set; }

		public ushort GetNumberIndex(double n) {
			int i;
			i = NumberLookupDouble.IndexOf(n);
			if (i >= 0)
				return (ushort)i;
			i = NumberLookupInt.IndexOf(n);
			if (i >= 0)
				return (ushort)(0xc000 | i);
			i = NumberLookupShort.IndexOf(n);
			if (i >= 0)
				return (ushort)(0x4000 | i);

			throw new InvalidDataException();
		}

		public byte[] GenerateWordIndexBytes(string text) {
			return StringUtils.GenerateWordIndexBytes(WordLookupList, text);
		}

	}
}
