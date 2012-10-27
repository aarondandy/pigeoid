// TODO: source header

using System.Collections.Generic;
using System.IO;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgArea :
		IRelatableIntersects<EpsgArea>,
		IRelatableContains<EpsgArea>,
		IRelatableWithin<EpsgArea>
	{

		internal class EpsgAreaLookUp : EpsgDynamicLookUpBase<ushort, EpsgArea>
		{

			private const string DatFileName = "areas.dat";
			private const string TxtFileName = "areas.txt";
			private const int FileHeaderSize = sizeof(ushort);
			private const int RecordDataSize = (4 * sizeof(short)) + sizeof(ushort);
			private const int RecordSize = sizeof(ushort) + RecordDataSize;
			private const int CodeSize = sizeof(ushort);
			
			private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

			private static ushort[] GetKeys() {
				using(var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					var keys = new ushort[reader.ReadUInt16()];
					for (int i = 0; i < keys.Length; i++) {
						keys[i] = reader.ReadUInt16();
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
					return keys;
				}
			}

			public EpsgAreaLookUp() : base(GetKeys()) { }

			private static double DecodeDegreeValueFromShort(short encoded) {
				double v = encoded / 100.0;
				while (v < -180 || v > 180) {
					v /= 10.0;
				}
				return v;
			}

			protected override EpsgArea Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + FileHeaderSize + CodeSize, SeekOrigin.Begin);
					var westBound = DecodeDegreeValueFromShort(reader.ReadInt16());
					var eastBound = DecodeDegreeValueFromShort(reader.ReadInt16());
					var southBound = DecodeDegreeValueFromShort(reader.ReadInt16());
					var northBound = DecodeDegreeValueFromShort(reader.ReadInt16());
					var name = TextLookUp.GetString(reader.ReadUInt16());
					return new EpsgArea(
						key, name,
						EpsgTextLookUp.LookUpIsoString(key, "iso2.dat", 2),
						EpsgTextLookUp.LookUpIsoString(key, "iso3.dat", 3),
						new LongitudeDegreeRange(westBound, eastBound),
						new Range(southBound, northBound)
					);
				}
			}

			protected override ushort GetKeyForItem(EpsgArea value) {
				return value._code;
			}

		}

		internal static readonly EpsgAreaLookUp LookUp = new EpsgAreaLookUp();

		public static EpsgArea Get(int code) {
			return LookUp.Get(checked((ushort) code));
		}

		public static IEnumerable<EpsgArea> Values { get { return LookUp.Values; } }

		private readonly ushort _code;
		private readonly string _iso2;
		private readonly string _iso3;
		private readonly string _name;
		private readonly LongitudeDegreeRange _longitudeRange;
		private readonly Range _latRange;

		internal EpsgArea(ushort code, string name, string iso2, string iso3, LongitudeDegreeRange longitudeRange, Range latRange) {
			_code = code;
			_name = name;
			_iso2 = iso2;
			_iso3 = iso3;
			_longitudeRange = longitudeRange;
			_latRange = latRange;
		}

		public int Code { get { return _code; } }

		public string Name { get { return _name; } }

		public string Iso2 { get { return _iso2; } }

		public string Iso3 { get { return _iso3; } }

		public LongitudeDegreeRange LongitudeRange { get { return _longitudeRange; } }

		public Range LatitudeRange { get { return _latRange; } }

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

		public bool Intersects(EpsgArea other) {
			return _longitudeRange.Intersects(other._longitudeRange)
				&& _latRange.Intersects(other._latRange);
		}

		public bool Contains(EpsgArea other) {
			return _longitudeRange.Contains(other._longitudeRange)
				&& _latRange.Contains(other._latRange);
		}

		public bool Within(EpsgArea other) {
			return _longitudeRange.Within(other._longitudeRange)
				&& _latRange.Within(other._latRange);
		}

	}
}
