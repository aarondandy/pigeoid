﻿// TODO: source header

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

		private static SortedDictionary<ushort, T> GenerateSimpleLookup<T>(
			string fileName, Func<ushort,string,EpsgArea,T> generate)
		{
			var lookup = new SortedDictionary<ushort, T>();
			using (var readerTxt = EpsgDataResource.CreateBinaryReader(TxtFileName))
			using (var readerDat = EpsgDataResource.CreateBinaryReader(fileName)) {
				while (readerDat.BaseStream.Position < readerDat.BaseStream.Length) {
					var code = readerDat.ReadUInt16();
					var name = EpsgTextLookup.GetString(readerDat.ReadUInt16(), readerTxt);
					var area = EpsgArea.Get(readerDat.ReadUInt16());
					lookup.Add(code, generate(code, name, area));
				}
			}
			return lookup;
		}

		internal class EpsgDatumEngineeringLookup : EpsgFixedLookupBase<ushort, EpsgDatumEngineering>
		{

			public static EpsgDatumEngineeringLookup Create() {
				return new EpsgDatumEngineeringLookup();
			}

			private static EpsgDatumEngineering Create(ushort code, string name, EpsgArea area) {
				return new EpsgDatumEngineering(code, name, area);
			}

			public EpsgDatumEngineeringLookup() : base(GenerateSimpleLookup("datumegr.dat", Create)) { }
		}

		internal class EpsgDatumVerticalLookup : EpsgFixedLookupBase<ushort, EpsgDatumVertical>
		{

			public static EpsgDatumVerticalLookup Create() {
				return new EpsgDatumVerticalLookup();
			}

			private static EpsgDatumVertical Create(ushort code, string name, EpsgArea area) {
				return new EpsgDatumVertical(code, name, area);
			}

			public EpsgDatumVerticalLookup() : base(GenerateSimpleLookup("datumver.dat",Create)) { }
		}

		internal class EpsgDatumGeodeticLookup : EpsgDynamicLookupBase<ushort, EpsgDatumGeodetic>
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

			public EpsgDatumGeodeticLookup() : base(GetAllKeys()) { }

			protected override EpsgDatumGeodetic Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), TxtFileName);
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

		private static readonly Lazy<EpsgDatumEngineeringLookup> _lookupEgr = new Lazy<EpsgDatumEngineeringLookup>(
			EpsgDatumEngineeringLookup.Create,
			LazyThreadSafetyMode.ExecutionAndPublication
		);

		private static readonly Lazy<EpsgDatumVerticalLookup> _lookupVer = new Lazy<EpsgDatumVerticalLookup>(
			EpsgDatumVerticalLookup.Create,
			LazyThreadSafetyMode.ExecutionAndPublication
		);

		internal static EpsgDatumEngineeringLookup LookupEngineering { get { return _lookupEgr.Value; } }

		internal static EpsgDatumVerticalLookup LookupVertical { get { return _lookupVer.Value; } }

		internal static readonly EpsgDatumGeodeticLookup LookupGeodetic = new EpsgDatumGeodeticLookup();

		public static EpsgDatum Get(int code) {
			if(code < 0 || code > UInt16.MaxValue)
				return null;
			return RawGet((ushort)code);
		}

		private static EpsgDatum RawGet(ushort code) {
			return LookupGeodetic.Get(code)
				?? LookupVertical.Get(code)
				?? LookupEngineering.Get(code)
				as EpsgDatum;
		}

		public static EpsgDatumGeodetic GetGeodetic(int code) {
			if (code < 0 || code > UInt16.MaxValue)
				return null;
			return LookupGeodetic.Get((ushort)code);
		}

		public static IEnumerable<EpsgDatum> Values {
			get {
				return LookupGeodetic.Values
					.Union<EpsgDatum>(LookupVertical.Values)
					.Union(LookupEngineering.Values)
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

		private ushort _code;
		private string _name;
		private EpsgArea _area;

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
		public EpsgDatumEngineering(ushort code, string name, EpsgArea area) : base(code, name, area) { }
		public override string Type { get { return "Engineering"; } }
	}

	public class EpsgDatumVertical : EpsgDatum
	{
		public EpsgDatumVertical(ushort code, string name, EpsgArea area) : base(code, name, area) { }
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