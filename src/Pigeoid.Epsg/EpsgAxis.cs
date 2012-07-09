// TODO: source header

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgAxis :
		IAxis
	{

		private class EpsgAxisSet
		{
			public ushort CsKey;
			public EpsgAxis[] Axes;

		}

		private class EpsgAxisSetLookup : EpsgDynamicLookupBase<ushort, EpsgAxisSet>
		{
			private const string DatFileName = "axis.dat";
			//private const int FileHeaderSize = sizeof(ushort);
			private const int AxisRecordSize = sizeof(ushort) * 4;
			private const int CodeSize = sizeof(ushort);

			private class KeyData
			{
				public ushort[] KeyLookup;
				public Dictionary<ushort, ushort> KeyAddress;
			}

			private static KeyData GetKeyData() {
				var keyData = new KeyData();
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					keyData.KeyLookup = new ushort[reader.ReadUInt16()];
					keyData.KeyAddress = new Dictionary<ushort, ushort>(keyData.KeyLookup.Length);
					for (int i = 0; i < keyData.KeyLookup.Length; i++) {
						var address = (ushort)reader.BaseStream.Position;
						var key = reader.ReadUInt16();
						keyData.KeyLookup[i] = key;
						keyData.KeyAddress[key] = address;
						var axesCount = reader.ReadByte();
						reader.BaseStream.Seek(axesCount * AxisRecordSize, SeekOrigin.Current);
					}
				}
				return keyData;
			}

			private readonly KeyData _keyData;

			public EpsgAxisSetLookup() : this(GetKeyData()) { }

			private EpsgAxisSetLookup(KeyData keyData) : base(keyData.KeyLookup) {
				_keyData = keyData;
			}

			protected override EpsgAxisSet Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek(_keyData.KeyAddress[key] + CodeSize, SeekOrigin.Begin);

					var axisSet = new EpsgAxisSet();
					axisSet.CsKey = key;
					axisSet.Axes = new EpsgAxis[reader.ReadByte()];
					for (int i = 0; i < axisSet.Axes.Length; i++) {
						var uom = EpsgUom.Get(reader.ReadUInt16());
						using (var textReader = EpsgDataResource.CreateBinaryReader("axis.txt")) {
							var name = EpsgTextLookup.GetString(reader.ReadUInt16(), textReader);
							var orientation = EpsgTextLookup.GetString(reader.ReadUInt16(), textReader);
							var abbreviation = EpsgTextLookup.GetString(reader.ReadUInt16(), textReader);
							axisSet.Axes[i] = new EpsgAxis(name,abbreviation,orientation,uom);
						}
					}

					return axisSet;
				}
			}

			protected override ushort GetKeyForItem(EpsgAxisSet value) {
				return value.CsKey;
			}

		}

		private static readonly EpsgAxisSetLookup _setLookup = new EpsgAxisSetLookup();

		internal static IEnumerable<EpsgAxis> Get(ushort csCode) {
			var set = _setLookup.Get(csCode);
			return set == null
				? Enumerable.Empty<EpsgAxis>()
				: Array.AsReadOnly(set.Axes);
		}

		private readonly string _name;
		private readonly string _abbreviation;
		private readonly string _orientation;
		private readonly EpsgUom _uom;

		private EpsgAxis(string name, string abbreviation, string orientation, EpsgUom uom) {
			_name = name;
			_abbreviation = abbreviation;
			_orientation = orientation;
			_uom = uom;
		}

		public EpsgUom  Unit { get { return _uom; } }

		public string Name { get { return _name; } }

		public string Abbreviation { get { return _abbreviation; } }

		public string Orientation { get { return _orientation; } }

	}
}
