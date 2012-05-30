// TODO: source header

using System;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgCoordOpMethodInfo
	{

		internal class EpsgCoordOpMethodInfoLookup : EpsgDynamicLookupBase<ushort, EpsgCoordOpMethodInfo>
		{
			private const string DatFileName = "opmethod.dat";
			private const int FileHeaderSize = sizeof(ushort);
			private const int RecordDataSize = sizeof(ushort) + sizeof(byte);
			private const int RecordSize = sizeof(ushort) + RecordDataSize;
			private const int CodeSize = sizeof(ushort);

			private static ushort[] GetKeys()
			{
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					var keys = new ushort[reader.ReadUInt16()];
					for (int i = 0; i < keys.Length; i++ ) {
						keys[i] = reader.ReadUInt16();
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
					return keys;
				}
			}

			public EpsgCoordOpMethodInfoLookup() : base(GetKeys()) { }

			protected override EpsgCoordOpMethodInfo Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + FileHeaderSize + CodeSize, SeekOrigin.Begin);
					var reverse = reader.ReadByte() == 'B';
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), "opmethod.txt");
					return new EpsgCoordOpMethodInfo(key, name, reverse);
				}
			}

			protected override ushort GetKeyForItem(EpsgCoordOpMethodInfo value) {
				return value._code;
			}
			
		}

		internal static readonly EpsgCoordOpMethodInfoLookup Lookup = new EpsgCoordOpMethodInfoLookup();

		public static EpsgCoordOpMethodInfo Get(int code) {
			return code >= 0 && code <= UInt16.MaxValue
				? Lookup.Get(unchecked((ushort)code))
				: null;
		}

		public static IEnumerable<EpsgCoordOpMethodInfo> Values { get { return Lookup.Values; } }

		private readonly ushort _code;
		private readonly string _name;
		private readonly bool _canReverse;

		private EpsgCoordOpMethodInfo(ushort code, string name, bool canReverse)
		{
			_code = code;
			_name = name;
			_canReverse = canReverse;
		}

		public int Code { get { return _code; } }
		public string Name { get { return _name; } }
		public bool CanReverse { get { return _canReverse; } }

	}
}
