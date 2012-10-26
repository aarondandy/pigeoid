using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgCrsProjected : EpsgCrs, ICrsProjected
	{

		internal class EpsgCrsProjectedLookUp : EpsgDynamicLookUpBase<int,EpsgCrsProjected>
		{
			private const string DatFileName = "crsprj.dat";
			private const string TxtFileName = "crs.txt";
			private const int RecordDataSize = (sizeof(ushort) * 5) + sizeof(byte);
			private const int CodeSize = sizeof(uint);
			private const int RecordSize = CodeSize + RecordDataSize;
			private const int RecordIndexSkipSize = RecordDataSize - sizeof(ushort);

			public static EpsgCrsProjectedLookUp Create() {
				var keys = new List<int>();
				var reverseIndex = new Dictionary<ushort, List<int>>();
				using(var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					while(reader.BaseStream.Position < reader.BaseStream.Length) {
						var key = (int)reader.ReadUInt32();
						var baseCrs = reader.ReadUInt16();
						reader.BaseStream.Seek(RecordIndexSkipSize, SeekOrigin.Current);

						keys.Add(key);
						List<int> codeList;
						if(!reverseIndex.TryGetValue(baseCrs,out codeList)) {
							codeList = new List<int>();
							reverseIndex.Add(baseCrs, codeList);
						}
						codeList.Add(key);
					}
				}
				return new EpsgCrsProjectedLookUp(keys.ToArray(), reverseIndex.ToDictionary(x => x.Key, x => x.Value.ToArray()));
			}

			private readonly Dictionary<ushort, int[]> _reverseIndex;

			private EpsgCrsProjectedLookUp(int[] keys, Dictionary<ushort, int[]> reverseIndex)
				: base(keys) {
				_reverseIndex = reverseIndex;
			}

			[CanBeNull]
			public ReadOnlyCollection<int> GetProjectionCodesBasedOn(int baseCrsCode) {
				int[] rawList;
				return baseCrsCode >= 0
					&& baseCrsCode <= ushort.MaxValue
					&& _reverseIndex.TryGetValue((ushort)baseCrsCode, out rawList)
					? Array.AsReadOnly(rawList)
					: null;
			}

			protected override EpsgCrsProjected Create(int code, int index) {
				using(var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
					var baseCrs = EpsgCrs.Get(reader.ReadUInt16());
					var projectionCode = reader.ReadUInt16();
					var cs = EpsgCoordinateSystem.Get(reader.ReadUInt16());
					var area = EpsgArea.Get(reader.ReadUInt16());
					var name = EpsgTextLookUp.GetString(reader.ReadUInt16(), TxtFileName);
					var deprecated = reader.ReadByte() == 0xff;
					return new EpsgCrsProjected(code, name, area, deprecated, baseCrs, cs, projectionCode);
				}
			}

			protected override int GetKeyForItem(EpsgCrsProjected value) {
				return value.Code;
			}
		}

		internal static readonly EpsgCrsProjectedLookUp LookUp = EpsgCrsProjectedLookUp.Create();

		public static EpsgCrsProjected GetProjected(int code) {
			return LookUp.Get(code);
		}

		public static IEnumerable<EpsgCrsProjected> ProjectedValues { get { return LookUp.Values; } }

		public static IEnumerable<int> GetProjectionCodesBasedOn(int baseCrsCode) {
			return LookUp.GetProjectionCodesBasedOn(baseCrsCode) ?? Enumerable.Empty<int>();
		}

		public static IEnumerable<EpsgCrsProjected> GetProjectionsBasedOn(int baseCrsCode) {
			return GetProjectionCodesBasedOn(baseCrsCode)
				.Select(GetProjected)
				.Where(x => null != x);
		}

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
		private readonly int _projectionCode;

		internal EpsgCrsProjected(int code, string name, EpsgArea area, bool deprecated, EpsgCrs baseCrs, EpsgCoordinateSystem cs, int projectionCode)
			: base(code, name, area, deprecated)
		{
			_base = baseCrs;
			_cs = cs;
			_projectionCode = projectionCode;
		}

		public EpsgCoordinateSystem CoordinateSystem { get { return _cs; } }

		public EpsgCrs BaseCrs { get { return _base; } }

		public EpsgCrsGeodetic BaseGeodeticCrs { get { return FindGeodeticBase(_base); } }

		ICrsGeodetic ICrsProjected.BaseCrs { get { return BaseGeodeticCrs; } }

		public EpsgCoordinateOperationInfo Projection {
			get { return EpsgCoordinateOperationInfoRepository.GetOperationInfo(_projectionCode); }
		}

		ICoordinateOperationInfo ICrsProjected.Projection {
			get { return Projection; }
		}

		public EpsgDatumGeodetic Datum { get { return BaseGeodeticCrs.GeodeticDatum; } }

		IDatumGeodetic ICrsGeodetic.Datum { get { return Datum; } }

		public EpsgUom Unit { get { return _cs.Axes.First().Unit; } }

		IUom ICrsGeodetic.Unit { get { return Unit; } }

		public IList<EpsgAxis> Axes { get { return _cs.Axes.ToArray(); } }

		IList<IAxis> ICrsGeodetic.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }
	}
}
