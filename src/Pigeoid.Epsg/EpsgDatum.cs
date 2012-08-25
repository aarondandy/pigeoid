// TODO: source header

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;
using Vertesaur.Contracts;

namespace Pigeoid.Epsg
{

	public static class EpsgDatumRepository
	{

		private const int CodeSize = sizeof(ushort);
		private const string TxtFileName = "datums.txt";

		private static SortedDictionary<ushort, T> GenerateSimpleLookUp<T>(
			string fileName, Func<ushort,string,EpsgArea,T> generate)
		{
			var lookUp = new SortedDictionary<ushort, T>();
			using (var readerTxt = EpsgDataResource.CreateBinaryReader(TxtFileName))
			using (var readerDat = EpsgDataResource.CreateBinaryReader(fileName)) {
				while (readerDat.BaseStream.Position < readerDat.BaseStream.Length) {
					var code = readerDat.ReadUInt16();
					var name = EpsgTextLookUp.GetString(readerDat.ReadUInt16(), readerTxt);
					var area = EpsgArea.Get(readerDat.ReadUInt16());
					lookUp.Add(code, generate(code, name, area));
				}
			}
			return lookUp;
		}

		internal class EpsgDatumGeodeticLookUp : EpsgDynamicLookUpBase<ushort, EpsgDatumGeodetic>
		{
			private const string DatFileName = "datumgeo.dat";
			private const int RecordDataSize = sizeof(ushort) * 4;
			private const int RecordSize = sizeof(ushort) + RecordDataSize;

			private static ushort[] GetAllKeys() {
				using(var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					var keys = new List<ushort>();
					while (reader.BaseStream.Position < reader.BaseStream.Length) {
						keys.Add(reader.ReadUInt16());
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
					return keys.ToArray();
				}
			}

			public EpsgDatumGeodeticLookUp() : base(GetAllKeys()) { }

			protected override EpsgDatumGeodetic Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
					var name = EpsgTextLookUp.GetString(reader.ReadUInt16(), TxtFileName);
					var area = EpsgArea.Get(reader.ReadUInt16());
					var spheroid = EpsgEllipsoid.Get(reader.ReadUInt16());
					var meridian = EpsgPrimeMeridian.Get(reader.ReadUInt16());
					return new EpsgDatumGeodetic(key, name, spheroid, meridian, area);
				}
			}

			protected override ushort GetKeyForItem(EpsgDatumGeodetic value) {
				return (ushort)value.Code;
			}
		}

		private static readonly Lazy<EpsgFixedLookUpBase<ushort, EpsgDatumEngineering>> LookUpEngineeringCore = new Lazy<EpsgFixedLookUpBase<ushort, EpsgDatumEngineering>>(
			() => new EpsgFixedLookUpBase<ushort, EpsgDatumEngineering>(GenerateSimpleLookUp(
				"datumegr.dat",
				(code, name, area) => new EpsgDatumEngineering(code, name, area)
			)),
			LazyThreadSafetyMode.ExecutionAndPublication
		);

		private static readonly Lazy<EpsgFixedLookUpBase<ushort, EpsgDatumVertical>> LookUpVerticalCore = new Lazy<EpsgFixedLookUpBase<ushort, EpsgDatumVertical>>(
			() => new EpsgFixedLookUpBase<ushort, EpsgDatumVertical>(GenerateSimpleLookUp(
				"datumver.dat",
				(code, name, area) => new EpsgDatumVertical(code, name, area)
			)),
			LazyThreadSafetyMode.ExecutionAndPublication
		);

		internal static EpsgFixedLookUpBase<ushort, EpsgDatumEngineering> LookUpEngineering { get { return LookUpEngineeringCore.Value; } }

		internal static EpsgFixedLookUpBase<ushort, EpsgDatumVertical> LookUpVertical { get { return LookUpVerticalCore.Value; } }

		internal static readonly EpsgDatumGeodeticLookUp LookUpGeodetic = new EpsgDatumGeodeticLookUp();

		public static EpsgDatum Get(int code) {
			return code >= 0 && code < UInt16.MaxValue
				? RawGet((ushort) code)
				: null;
		}

		private static EpsgDatum RawGet(ushort code) {
			return LookUpGeodetic.Get(code)
				?? LookUpVertical.Get(code)
				?? LookUpEngineering.Get(code)
				as EpsgDatum;
		}

		public static EpsgDatumGeodetic GetGeodetic(int code) {
			return code >= 0 && code < UInt16.MaxValue
				? LookUpGeodetic.Get((ushort) code)
				: null;
		}

		public static IEnumerable<EpsgDatum> Values {
			get {
				return LookUpGeodetic.Values
					.Union<EpsgDatum>(LookUpVertical.Values)
					.Union(LookUpEngineering.Values)
					.OrderBy(x => x.Code);
			}
		}

	}

	public abstract class EpsgDatum : IDatum
	{

		public static IEnumerable<EpsgDatum> Values { get { return EpsgDatumRepository.Values; } }

		public static EpsgDatum Get(int code) {
			return EpsgDatumRepository.Get(code);
		}

		private readonly ushort _code;
		private readonly string _name;
		private readonly EpsgArea _area;

		internal EpsgDatum(ushort code, string name, EpsgArea area) {
			_code = code;
			_name = name;
			_area = area;
		}

		public int Code { get { return _code; } }

		public string Name { get { return _name; } }

		public EpsgArea Area { get { return _area; } }

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

		public abstract string Type { get; }

	}

	public class EpsgDatumEngineering : EpsgDatum
	{
		internal EpsgDatumEngineering(ushort code, string name, EpsgArea area) : base(code, name, area) { }
		public override string Type { get { return "Engineering"; } }
	}

	public class EpsgDatumVertical : EpsgDatum
	{
		internal EpsgDatumVertical(ushort code, string name, EpsgArea area) : base(code, name, area) { }
		public override string Type { get { return "Vertical"; } }
	}

	public class EpsgDatumGeodetic : EpsgDatum, IDatumGeodetic
	{

		private readonly EpsgEllipsoid _spheroid;
		private readonly EpsgPrimeMeridian _primeMeridian;

		internal EpsgDatumGeodetic(ushort code, string name, EpsgEllipsoid spheroid, EpsgPrimeMeridian primeMeridian, EpsgArea area)
			: base(code, name, area)
		{
			_spheroid = spheroid;
			_primeMeridian = primeMeridian;
		}

		public EpsgEllipsoid Spheroid {
			get { return _spheroid; }
		}

		public EpsgPrimeMeridian PrimeMeridian {
			get { return _primeMeridian; }
		}

		ISpheroid<double> IDatumGeodetic.Spheroid {
			get { return _spheroid; }
		}

		IPrimeMeridian IDatumGeodetic.PrimeMeridian {
			get { return _primeMeridian; }
		}

		public override string Type { get { return "Geodetic"; } }

	}
}
