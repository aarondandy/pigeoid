using System;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgCrsCompound : EpsgCrs, ICrsCompound
	{

		internal class EpsgCrsCompoundLookUp : EpsgDynamicLookUpBase<ushort,EpsgCrsCompound>
		{

			private const string DatFileName = "crscmp.dat";
			private const string TxtFileName = "crs.txt";
			private const int RecordDataSize = (sizeof(ushort) * 4) + sizeof(byte);
			private const int CodeSize = sizeof(ushort);
			private const int RecordSize = CodeSize + RecordDataSize;

			private static ushort[] GetKeys() {
				var keys = new List<ushort>();
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					while (reader.BaseStream.Position < reader.BaseStream.Length) {
						keys.Add(reader.ReadUInt16());
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
				}
				return keys.ToArray();
			}

			public EpsgCrsCompoundLookUp() : base(GetKeys()) { }

			protected override EpsgCrsCompound Create(ushort code, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
					var horizontal = EpsgCrs.Get(reader.ReadUInt16());
					var vertical = (EpsgCrsVertical)EpsgCrsDatumBased.GetDatumBased(reader.ReadUInt16());
					var area = EpsgArea.Get(reader.ReadUInt16());
					var name = EpsgTextLookUp.GetString(reader.ReadUInt16(), TxtFileName);
					var deprecated = reader.ReadByte() == 0xff;
					return new EpsgCrsCompound(code, name, area, deprecated, horizontal, vertical);
				}
			}

			protected override ushort GetKeyForItem(EpsgCrsCompound value) {
				return (ushort)value.Code;
			}

		}

		internal static readonly EpsgCrsCompoundLookUp LookUp = new EpsgCrsCompoundLookUp();

		public static EpsgCrsCompound GetCompound(int code) {
			return code >= 0 && code <= UInt16.MaxValue
				? LookUp.Get((ushort) code)
				: null;
		}

		public static IEnumerable<EpsgCrsCompound> CompoundValues { get { return LookUp.Values; } }

		private readonly EpsgCrs _horizontal;
		private readonly EpsgCrsVertical _vertical;

		private EpsgCrsCompound(
			int code, string name, EpsgArea area, bool deprecated,
			EpsgCrs horizontal, EpsgCrsVertical vertical
		) : base(code,name,area,deprecated) {
			_horizontal = horizontal;
			_vertical = vertical;
		}

		public EpsgCrs Horizontal { get { return _horizontal; } }

		ICrs ICrsCompound.Head { get { return _horizontal; } }

		public EpsgCrsVertical Vertical { get { return _vertical; } }

		ICrs ICrsCompound.Tail { get { return _vertical; } }

	}
}
