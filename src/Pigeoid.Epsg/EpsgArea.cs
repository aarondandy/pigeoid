// TODO: source header

using System;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgArea
	{

		internal class EpsgAreaLookup : EpsgDynamicLookupBase<ushort, EpsgArea>
		{

			private const string DatFileName = "areas.dat";
			private const string TxtFileName = "areas.txt";
			private const int FileHeaderSize = sizeof(ushort);
			private const int RecordDataSize = (4 * sizeof(short)) + sizeof(ushort);
			private const int RecordSize = sizeof(ushort) + RecordDataSize;

			private static ushort[] GetAllKeys() {
				using(var reader = EpsgDataResource.CreteBinaryReader(DatFileName)) {
					var keys = new ushort[reader.ReadUInt16()];
					for (int i = 0; i < keys.Length; i++) {
						keys[i] = reader.ReadUInt16();
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
					return keys;
				}
			}

			public EpsgAreaLookup() : base(GetAllKeys()) { }

			protected override EpsgArea Create(ushort key) {
				var keyIndex = GetKeyIndex(key);
				var recordAddress = FileHeaderSize + (keyIndex*RecordSize);

				using (var reader = EpsgDataResource.CreteBinaryReader(DatFileName)) {
					reader.BaseStream.Seek(recordAddress, SeekOrigin.Begin);
					var code = reader.ReadUInt16();
					var westBound = reader.ReadInt16() / 100.0;
					var eastBound = reader.ReadInt16() / 100.0;
					var southBound = reader.ReadInt16() / 100.0;
					var northBound = reader.ReadInt16() / 100.0;
					var stringOffset = reader.ReadUInt16();

					var name = stringOffset != UInt16.MaxValue
						? EpsgTextLookup.GetString(stringOffset, TxtFileName)
						: String.Empty;

					var iso2 = EpsgTextLookup.LookupIsoString(code, "iso2.dat", 2);
					var iso3 = EpsgTextLookup.LookupIsoString(code, "iso3.dat", 3);

					return new EpsgArea(code,name,iso2,iso3);
				}
			}

			protected override ushort GetKeyForItem(EpsgArea value) {
				return value._code;
			}

		}

		internal static readonly EpsgAreaLookup Lookup = new EpsgAreaLookup();

		[CLSCompliant(false)]
		public static EpsgArea Get(ushort code) {
			return Lookup.Get(code);
		}

		public static EpsgArea Get(int code) {
			return Get(checked((ushort) code));
		}

		public static IEnumerable<EpsgArea> Values { get { return Lookup.Values; } }

		private readonly ushort _code;
		private readonly string _iso2;
		private readonly string _iso3;
		private readonly string _name;

		internal EpsgArea(ushort code, string name, string iso2, string iso3) {
			_code = code;
			_name = name;
			_iso2 = iso2;
			_iso3 = iso3;
		}

		public int Code { get { return _code; } }

		public string Name { get { return _name; } }

		public string Iso2 { get { return _iso2; } }

		public string Iso3 { get { return _iso3; } }

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

	}
}
