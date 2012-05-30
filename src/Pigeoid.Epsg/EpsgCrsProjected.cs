using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgCrsProjected : EpsgCrs, ICrsProjected
	{

		internal class EpsgCrsProjectedLookup : EpsgDynamicLookupBase<int,EpsgCrsProjected>
		{
			private const string DatFileName = "crsprj.dat";
			private const string TxtFileName = "crs.txt";
			private const int RecordDataSize = (sizeof(ushort) * 5) + sizeof(byte);
			private const int CodeSize = sizeof(uint);
			private const int RecordSize = CodeSize + RecordDataSize;

			private static int[] GetKeys() {
				var keys = new List<int>();
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					while (reader.BaseStream.Position < reader.BaseStream.Length) {
						keys.Add((int)reader.ReadUInt32());
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
				}
				return keys.ToArray();
			}

			public EpsgCrsProjectedLookup() : base(GetKeys()) { }

			protected override EpsgCrsProjected Create(int code, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
					var baseCrs = EpsgCrs.Get(reader.ReadUInt16());
					var projCode = reader.ReadUInt16(); // TODO: get the projection data
					var cs = EpsgCoordinateSystem.Get(reader.ReadUInt16());
					var area = EpsgArea.Get(reader.ReadUInt16());
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), TxtFileName);
					var deprecated = reader.ReadByte() == 0xff;
					return new EpsgCrsProjected(code, name, area, deprecated, baseCrs, cs);
				}
			}

			protected override int GetKeyForItem(EpsgCrsProjected value) {
				return value.Code;
			}
		}

		internal static readonly EpsgCrsProjectedLookup Lookup = new EpsgCrsProjectedLookup();

		public static EpsgCrsProjected Get(int code) {
			return Lookup.Get(code);
		}

		public static IEnumerable<EpsgCrsProjected> Values { get { return Lookup.Values; } }

		private static EpsgCrsGeodetic FindGeodeticBase(EpsgCrs crs) {
			do {
				if (crs is EpsgCrsGeodetic)
					return crs as EpsgCrsGeodetic;

				if (crs is EpsgCrsProjected)
					crs = (crs as EpsgCrsProjected)._base;

			} while (null != crs);
			return null;
		}

		private readonly EpsgCrs _base;
		private readonly EpsgCoordinateSystem _cs;

		internal EpsgCrsProjected(int code, string name, EpsgArea area, bool deprecated, EpsgCrs baseCrs, EpsgCoordinateSystem cs)
			: base(code, name, area, deprecated)
		{
			_base = baseCrs;
			_cs = cs;
		}

		public EpsgCoordinateSystem CoordinateSystem { get { return _cs; } }

		public EpsgCrs BaseCrs { get { return _base; } }

		public EpsgCrsGeodetic BaseGeodeticCrs { get { return FindGeodeticBase(_base); } }

		ICrsGeodetic ICrsProjected.BaseCrs { get { return BaseGeodeticCrs; } }

		public Vertesaur.Contracts.ITransformation Projection {
			get { throw new NotImplementedException(); }
		}

		public EpsgDatumGeodetic Datum { get { return BaseGeodeticCrs.GeodeticDatum; } }

		IDatumGeodetic ICrsGeodetic.Datum { get { return Datum; } }

		public EpsgUom Unit { get { return _cs.Axes.First().Unit; } }

		IUom ICrsGeodetic.Unit { get { return Unit; } }

		public IList<EpsgAxis> Axes { get { return _cs.Axes.ToArray(); } }

		IList<IAxis> ICrsGeodetic.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }
	}
}
