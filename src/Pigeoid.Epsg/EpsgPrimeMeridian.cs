// TODO: source header

using System.Collections.Generic;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgPrimeMeridian :
		IPrimeMeridian
	{

		internal class EpsgPrimeMeridianLookup : EpsgFixedLookupBase<ushort, EpsgPrimeMeridian>
		{
			private static SortedDictionary<ushort,EpsgPrimeMeridian> GenerateLookup() {
				var lookup = new SortedDictionary<ushort, EpsgPrimeMeridian>();
				using (var readerTxt = EpsgDataResource.CreateBinaryReader("meridians.txt"))
				using (var numberLookup = new EpsgNumberLookup())
				using (var readerDat = EpsgDataResource.CreateBinaryReader("meridians.dat")) {
					for (int i = readerDat.ReadUInt16(); i > 0; i--) {
						var code = readerDat.ReadUInt16();
						var uom = EpsgUom.Get(readerDat.ReadUInt16());
						var lon = numberLookup.Get(readerDat.ReadUInt16());
						var name = EpsgTextLookup.GetString(readerDat.ReadByte(), readerTxt);
						lookup.Add(code, new EpsgPrimeMeridian(code, name, lon, uom));
					}
				}
				return lookup;
			}

			public EpsgPrimeMeridianLookup() : base(GenerateLookup()) { }

		}


		internal static readonly EpsgPrimeMeridianLookup Lookup = new EpsgPrimeMeridianLookup();

		public static EpsgPrimeMeridian Get(int code) {
			return Lookup.Get(checked((ushort)code));
		}

		public static IEnumerable<EpsgPrimeMeridian> Values { get { return Lookup.Values; } }

		private readonly ushort _code;
		private readonly EpsgUom _uom;
		private readonly double _lon;
		private readonly string _name;

		private EpsgPrimeMeridian(ushort code, string name, double lon, EpsgUom uom) {
			_code = code;
			_uom = uom;
			_lon = lon;
			_name = name;
		}

		public int Code {
			get { return _code; }
		}

		public string Name {
			get { return _name; }
		}

		public double Longitude {
			get { return _lon; }
		}

		public EpsgUom Unit {
			get { return _uom; }
		}

		IUom IPrimeMeridian.Unit {
			get { return _uom; }
		}

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

	}
}
