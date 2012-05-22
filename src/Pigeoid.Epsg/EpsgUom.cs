using System;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgUom :
		IUom
	{

		internal class EpsgUomLookup : EpsgFixedLookupBase<ushort,EpsgUom>
		{

			private static SortedDictionary<ushort,EpsgUom> GenerateLookup() {
				var lookup = new SortedDictionary<ushort, EpsgUom>();
				using (var readerTxt = EpsgDataResource.CreateBinaryReader("uoms.txt"))
				using (var numberLookup = new EpsgNumberLookup())
				{
					PopulateFromFile("uomang.dat", "Angle", readerTxt, numberLookup, lookup);
					PopulateFromFile("uomlen.dat", "Length", readerTxt, numberLookup, lookup);
					PopulateFromFile("uomscl.dat", "Scale", readerTxt, numberLookup, lookup);
				}
				return lookup;
			}

			private static void PopulateFromFile(string datFileName, string typeName, BinaryReader readerTxt, EpsgNumberLookup numberLookup, SortedDictionary<ushort, EpsgUom> lookup) {
				using (var readerDat = EpsgDataResource.CreateBinaryReader(datFileName)) {
					while (readerDat.BaseStream.Position < readerDat.BaseStream.Length) {
						var code = readerDat.ReadUInt16();
						var stringOffset = readerDat.ReadUInt16();
						var factorB = numberLookup.Get(readerDat.ReadUInt16());
						var factorC = numberLookup.Get(readerDat.ReadUInt16());
						var name = stringOffset != UInt16.MaxValue
							? EpsgTextLookup.GetString(stringOffset, readerTxt)
							: String.Empty;
						lookup.Add(code, new EpsgUom(code, name, typeName, factorB, factorC));
					}
				}
			}

			public EpsgUomLookup() : base(GenerateLookup()) { }
		}

		internal static readonly EpsgUomLookup Lookup = new EpsgUomLookup();

		[CLSCompliant(false)]
		public static EpsgUom Get(ushort code) {
			return Lookup.Get(code);
		}

		public static EpsgUom Get(int code) {
			return Get(checked((ushort)code));
		}

		public static IEnumerable<EpsgUom> Values { get { return Lookup.Values; } }

		private readonly ushort _code;
		private readonly string _name;
		private readonly double _factorB;
		private readonly double _factorC;
		private readonly string _type;

		private EpsgUom(ushort code, string name, string type, double factorB, double factorC) {
			_code = code;
			_name = name;
			_factorB = factorB;
			_factorC = factorC;
			_type = type;
		}

		public int Code { get { return _code; } }

		public double FactorB { get { return _factorB; } }

		public double FactorC { get { return _factorC; } }

		public string Name { get { return _name; } }

		public string Type { get { return _type; } }

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

	}
}
