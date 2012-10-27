// TODO: source header

using System;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Contracts;
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

		internal class EpsgCoordinateSystemLookUp : EpsgDynamicLookUpBase<ushort, EpsgCoordinateSystem>
		{

			private const string DatFileName = "coordsys.dat";
			private const string TxtFileName = "coordsys.txt";
			private const int RecordDataSize = sizeof(ushort) + sizeof(byte);
			private const int RecordSize = sizeof(ushort) + RecordDataSize;
			private const int HeaderSize = sizeof(ushort);
			private const int CodeSize = sizeof(ushort);

			private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

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

			public EpsgCoordinateSystemLookUp() : base(GetAllKeys()) { }

			private static CsType DecodeCsType(byte value) {
				switch (value & 0x70) {
					case 0x10: return CsType.Cartesian;
					case 0x20: return CsType.Ellipsoidal;
					case 0x30: return CsType.Spherical;
					case 0x40: return CsType.Vertical;
					default: return CsType.None;
				}
			}

			protected override EpsgCoordinateSystem Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + HeaderSize + CodeSize, SeekOrigin.Begin);
					var typeData = reader.ReadByte();
					var name = TextLookUp.GetString(reader.ReadUInt16());
					return new EpsgCoordinateSystem(
						key, name,
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

		internal static readonly EpsgCoordinateSystemLookUp LookUp = new EpsgCoordinateSystemLookUp();

		public static EpsgCoordinateSystem Get(int code) {
			return code >= 0 && code <= UInt16.MaxValue
				? LookUp.Get(unchecked((ushort)code))
				: null;
		}

		public static IEnumerable<EpsgCoordinateSystem> Values { get { return LookUp.Values; } }

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

		public IEnumerable<EpsgAxis> Axes { get { return EpsgAxis.Get(_code) ; } }

		public IAuthorityTag Authority { get { return new EpsgAuthorityTag(_code); } }

	}
}
