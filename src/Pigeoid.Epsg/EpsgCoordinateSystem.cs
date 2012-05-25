

using System;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgCoordinateSystem
	{
		[Flags]
		public enum CsType : byte
		{
			None = 0,
			Cartesian = 1,
			Ellipsoidal = 2,
			Spherical = Cartesian | Ellipsoidal, // 3
			Vertical = 4
		}

		internal class EpsgCoordinateSystemLookup : EpsgDynamicLookupBase<ushort, EpsgCoordinateSystem>
		{

			private const string DatFileName = "coordsys.dat";
			private const int RecordDataSize = sizeof(ushort) + sizeof(byte);
			private const int RecordSize = sizeof(ushort) + RecordDataSize;
			private const int HeaderSize = sizeof(ushort);

			private static ushort[] GetAllKeys() {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					var keys = new ushort[reader.ReadUInt16()];
					for (int i = 0; i < keys.Length; i++) {
						keys[i] = reader.ReadUInt16();
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
					return keys;
				}
			}

			public EpsgCoordinateSystemLookup() : base(GetAllKeys()) { }

			private static CsType DecodeCsType(byte value) {
				switch (value & 0x70) {
					case 0x10: return CsType.Cartesian;
					case 0x20: return CsType.Ellipsoidal;
					case 0x30: return CsType.Spherical;
					case 0x40: return CsType.Vertical;
					default: return CsType.None;
				}
			}

			protected override EpsgCoordinateSystem Create(ushort key) {
				var keyIndex = GetKeyIndex(key);
				var recordAddress = (keyIndex * RecordSize) + HeaderSize;
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek(recordAddress, SeekOrigin.Begin);
					var code = reader.ReadUInt16();
					var typeData = reader.ReadByte();
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), "coordsys.txt");
					return new EpsgCoordinateSystem(
						code, name,
						dimension: typeData & 3,
						deprecated: 0 != (typeData & 128),
						csType: DecodeCsType(typeData)
					);
				}
			}

			protected override ushort GetKeyForItem(EpsgCoordinateSystem value) {
				return (ushort)value.Code;
			}
		}

		internal static readonly EpsgCoordinateSystemLookup Lookup = new EpsgCoordinateSystemLookup();

		public static EpsgCoordinateSystem Get(int code) {
			return Lookup.Get(checked((ushort)code));
		}

		public static IEnumerable<EpsgCoordinateSystem> Values { get { return Lookup.Values; } }

		private readonly ushort _code;
		private readonly int _dimension;
		// TODO: coordinate system type
		private readonly string _name;
		private readonly bool _deprecated;
		private readonly CsType _csType;

		private EpsgCoordinateSystem(ushort code, string name, int dimension, bool deprecated, CsType csType) {
			_code = code;
			_name = name;
			_dimension = dimension;
			_deprecated = deprecated;
			_csType = csType;
		}

		public int Code { get { return _code; } }

		public string Name { get { return _name; } }

		public int Dimension { get { return _dimension; } }

		public bool Deprecated { get { return _deprecated; } }

		public CsType Type { get { return _csType; } }


	}
}
