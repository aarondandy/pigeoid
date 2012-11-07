// TODO: source header

using System.Collections.Generic;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgPrimeMeridian : IPrimeMeridianInfo
	{

		internal static readonly EpsgFixedLookUpBase<ushort, EpsgPrimeMeridian> LookUp;

		static EpsgPrimeMeridian() {
			var lookUpDictionary = new SortedDictionary<ushort, EpsgPrimeMeridian>();
			using (var readerTxt = EpsgDataResource.CreateBinaryReader("meridians.txt"))
			using (var numberLookUp = new EpsgNumberLookUp())
			using (var readerDat = EpsgDataResource.CreateBinaryReader("meridians.dat")) {
				for (int i = readerDat.ReadUInt16(); i > 0; i--) {
					var code = readerDat.ReadUInt16();
					var uom = EpsgUnit.Get(readerDat.ReadUInt16());
					var longitude = numberLookUp.Get(readerDat.ReadUInt16());
					var name = EpsgTextLookUp.GetString(readerDat.ReadByte(), readerTxt);
					lookUpDictionary.Add(code, new EpsgPrimeMeridian(code, name, longitude, uom));
				}
			}
			LookUp = new EpsgFixedLookUpBase<ushort, EpsgPrimeMeridian>(lookUpDictionary);
		}

		public static EpsgPrimeMeridian Get(int code) {
			return code >= 0 && code < ushort.MaxValue ? LookUp.Get((ushort) code) : null;
		}

		public static IEnumerable<EpsgPrimeMeridian> Values { get { return LookUp.Values; } }

		private readonly ushort _code;
		private readonly EpsgUnit _unit;
		private readonly double _longitude;
		private readonly string _name;

		private EpsgPrimeMeridian(ushort code, string name, double longitude, EpsgUnit unit) {
			_code = code;
			_unit = unit;
			_longitude = longitude;
			_name = name;
		}

		public int Code {
			get { return _code; }
		}

		public string Name {
			get { return _name; }
		}

		public double Longitude {
			get { return _longitude; }
		}

		public EpsgUnit Unit {
			get { return _unit; }
		}

		IUnit IPrimeMeridianInfo.Unit {
			get { return _unit; }
		}

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

	}
}
