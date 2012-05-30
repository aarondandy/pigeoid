// TODO: source header

using System.Collections.Generic;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{

	public class EpsgParameterInfo
	{

		internal class EpsgParameterInfoLookup : EpsgDynamicLookupBase<ushort, EpsgParameterInfo>
		{
			private const string DatFileName = "parameters.dat";
			private const string TxtFileName = "parameters.txt";
			private const int FileHeaderSize = sizeof(ushort);
			private const int RecordDataSize = sizeof(short);
			private const int RecordSize = sizeof(ushort) + RecordDataSize;
			private const int CodeSize = sizeof(ushort);

			private static ushort[] GetKeys() {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					var keys = new ushort[reader.ReadUInt16()];
					for (int i = 0; i < keys.Length; i++) {
						keys[i] = reader.ReadUInt16();
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
					return keys;
				}
			}

			public EpsgParameterInfoLookup() : base(GetKeys()) { }

			protected override EpsgParameterInfo Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + FileHeaderSize + CodeSize, SeekOrigin.Begin);
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), TxtFileName);
					return new EpsgParameterInfo(key, name);
				}
			}

			protected override ushort GetKeyForItem(EpsgParameterInfo value) {
				return value._code;
			}
		}

		internal static readonly EpsgParameterInfoLookup Lookup = new EpsgParameterInfoLookup();

		public static EpsgParameterInfo Get(int code) {
			return Lookup.Get(checked((ushort)code));
		}

		public static IEnumerable<EpsgParameterInfo> Values { get { return Lookup.Values; } }

		private readonly ushort _code;
		private readonly string _name;

		internal EpsgParameterInfo(ushort code, string name) {
			_code = code;
			_name = name;
		}

		public int Code {
			get { return _code; }
		}

		public string Name {
			get { return _name; }
		}

	}

}
