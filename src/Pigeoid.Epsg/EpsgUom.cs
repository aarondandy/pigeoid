// TODO: source header

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgUom : IUom, IAuthorityBoundEntity
	{

		internal static readonly EpsgFixedLookUpBase<ushort, EpsgUom> LookUp;

		static EpsgUom() {
			var lookUpDictionary = new SortedDictionary<ushort, EpsgUom>();
			using (var readerTxt = EpsgDataResource.CreateBinaryReader("uoms.txt"))
			using (var numberLookUp = new EpsgNumberLookUp()) {
				PopulateFromFile("uomang.dat", "Angle", readerTxt, numberLookUp, lookUpDictionary);
				PopulateFromFile("uomlen.dat", "Length", readerTxt, numberLookUp, lookUpDictionary);
				PopulateFromFile("uomscl.dat", "Scale", readerTxt, numberLookUp, lookUpDictionary);
			}
			LookUp = new EpsgFixedLookUpBase<ushort, EpsgUom>(lookUpDictionary);
		}

		private static void PopulateFromFile(string datFileName, string typeName, BinaryReader readerTxt, EpsgNumberLookUp numberLookUp, SortedDictionary<ushort, EpsgUom> lookUpDictionary) {
			using (var readerDat = EpsgDataResource.CreateBinaryReader(datFileName)) {
				while (readerDat.BaseStream.Position < readerDat.BaseStream.Length) {
					var code = readerDat.ReadUInt16();
					var name = EpsgTextLookUp.GetString(readerDat.ReadUInt16(), readerTxt);
					var factorB = numberLookUp.Get(readerDat.ReadUInt16());
					var factorC = numberLookUp.Get(readerDat.ReadUInt16());
					lookUpDictionary.Add(code, new EpsgUom(code, name, typeName, factorB, factorC));
				}
			}
		}

		public static EpsgUom Get(int code) {
			return code >= 0 && code < ushort.MaxValue
				? LookUp.Get((ushort) code)
				: null;
		}

		public static IEnumerable<EpsgUom> Values { get { return LookUp.Values; } }

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

		private IUom GetBaseUnit() {
			if (StringComparer.OrdinalIgnoreCase.Equals("Angle", Type))
				return null;
			if (StringComparer.OrdinalIgnoreCase.Equals("Length", Type))
				return null;
			if (StringComparer.OrdinalIgnoreCase.Equals("Scale", Type))
				return null;
			return null;
		}

		public IEnumerable<IUom> ConvertibleTo { get { return new[]{ GetBaseUnit() }; } }

		public IEnumerable<IUom> ConvertibleFrom { get { return new[] { GetBaseUnit() }; } }

		public IUomConversion<double> GetConversionTo(IUom uom) {
			// ReSharper disable CompareOfFloatsByEqualityOperator
			if(null == uom)
				throw new ArgumentNullException("uom");
			if (0 == FactorC)
				return null;

			if (ConvertibleTo.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, uom.Name))) {
				if(FactorB == 1.0 && FactorC == 1.0)
					return new UomUnityConversion(this, uom);
				return new UomRatioConversion(this, uom, FactorB, FactorC);
			}
			return null;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public IUomConversion<double> GetConversionFrom(IUom uom) {
			var conversion = GetConversionTo(uom);
			return null != conversion && conversion.HasInverse ? conversion.GetInverse() : null;
		}
	}
}
