using Pigeoid.Epsg.DbRepository;
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

		public List<string> WordLookUpList { get; set; }

		public void SetNumberLists(IEnumerable<double> numbers) {
// ReSharper disable CompareOfFloatsByEqualityOperator
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
			NumberLookUpDouble = dList;
			NumberLookUpInt = iList;
			NumberLookUpShort = sList;
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public List<double> NumberLookUpDouble { get; private set; }

		public List<double> NumberLookUpInt { get; private set; }

		public List<double> NumberLookUpShort { get; private set; }

		public ushort GetNumberIndex(double n) {
			var i = NumberLookUpDouble.IndexOf(n);
			if (i >= 0)
				return (ushort)i;
			i = NumberLookUpInt.IndexOf(n);
			if (i >= 0)
				return (ushort)(0xc000 | i);
			i = NumberLookUpShort.IndexOf(n);
			if (i >= 0)
				return (ushort)(0x4000 | i);

			throw new InvalidDataException();
		}

		public byte[] GenerateWordIndexBytes(string text) {
			return StringUtils.GenerateWordIndexBytes(WordLookUpList, text);
		}

	}
}
