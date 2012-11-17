// TODO: source header

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
	public class EpsgUnit : IUnit, IAuthorityBoundEntity
	{

		internal static readonly EpsgFixedLookUpBase<ushort, EpsgUnit> LookUp;
		private static readonly ReadOnlyUnitConversionMap AllConversionMap;

		static EpsgUnit() {
			var lookUpDictionary = new SortedDictionary<ushort, EpsgUnit>();
			using (var readerTxt = EpsgDataResource.CreateBinaryReader("uoms.txt"))
			using (var numberLookUp = new EpsgNumberLookUp()) {
				PopulateFromFile("uomang.dat", "Angle", readerTxt, numberLookUp, lookUpDictionary);
				PopulateFromFile("uomlen.dat", "Length", readerTxt, numberLookUp, lookUpDictionary);
				PopulateFromFile("uomscl.dat", "Scale", readerTxt, numberLookUp, lookUpDictionary);
			}
			LookUp = new EpsgFixedLookUpBase<ushort, EpsgUnit>(lookUpDictionary);
			AllConversionMap = new ReadOnlyUnitConversionMap(
				lookUpDictionary.Values
					.Select(x => x.BuildConversionToBase())
					.Where(x => null != x)
					.Concat(new IUnitConversion<double>[] {
						new UnitScalarConversion(LookUp.Get(9014), LookUp.Get(9002), 6), // fathom to foot
						new UnitScalarConversion(LookUp.Get(9093), LookUp.Get(9002), 5280), // mile to foot
						new UnitScalarConversion(LookUp.Get(9096), LookUp.Get(9002), 3), // yard to foot
						new UnitScalarConversion(LookUp.Get(9097), LookUp.Get(9096), 22), // chain to yard
						new UnitScalarConversion(LookUp.Get(9097), LookUp.Get(9098), 100), // chain to link

						new UnitScalarConversion(LookUp.Get(9037), LookUp.Get(9005), 3),		// clarke yard to clarke foot
						new UnitScalarConversion(LookUp.Get(9038), LookUp.Get(9037), 22),	// clarke chain to clarke yard
						new UnitScalarConversion(LookUp.Get(9038), LookUp.Get(9039), 100),	// clarke chain to clarke link

						new UnitScalarConversion(LookUp.Get(9040), LookUp.Get(9041), 3),	// British yard to foot (Sears 1922)
						new UnitScalarConversion(LookUp.Get(9042), LookUp.Get(9040), 22),	// British chain to yard (Sears 1922)
						new UnitScalarConversion(LookUp.Get(9042), LookUp.Get(9043), 100),	// British chain to link (Sears 1922)

						new UnitScalarConversion(LookUp.Get(9050), LookUp.Get(9051), 3),	// British yard to foot (Benoit 1895 A)
						new UnitScalarConversion(LookUp.Get(9052), LookUp.Get(9050), 22),	// British chain to yard (Benoit 1895 A)
						new UnitScalarConversion(LookUp.Get(9052), LookUp.Get(9053), 100),	// British chain to link (Benoit 1895 A)

						new UnitScalarConversion(LookUp.Get(9060), LookUp.Get(9061), 3),	// British yard to foot (Benoit 1895 B)
						new UnitScalarConversion(LookUp.Get(9062), LookUp.Get(9060), 22),	// British chain to yard (Benoit 1895 B)
						new UnitScalarConversion(LookUp.Get(9062), LookUp.Get(9063), 100),	// British chain to link (Benoit 1895 B)

						new UnitScalarConversion(LookUp.Get(9084), LookUp.Get(9080), 3),	// Indian yard to foot
						new UnitScalarConversion(LookUp.Get(9085), LookUp.Get(9081), 3),	// Indian yard to foot (1937)
						new UnitScalarConversion(LookUp.Get(9086), LookUp.Get(9082), 3),	// Indian yard to foot (1962)
						new UnitScalarConversion(LookUp.Get(9087), LookUp.Get(9083), 3),	// Indian yard to foot (1975)

						new UnitScalarConversion(LookUp.Get(9099), LookUp.Get(9300), 3),	// British yard to foot (Sears 1922 truncated)
						new UnitScalarConversion(LookUp.Get(9301), LookUp.Get(9099), 22),	// British chain to yard (Sears 1922 truncated)
						new UnitScalarConversion(LookUp.Get(9301), LookUp.Get(9302), 100),	// British chain to link (Sears 1922 truncated)

						new UnitRatioConversion(LookUp.Get(9080), LookUp.Get(9070), 49999783, 50000000), // indian foot to british foot 1865

						new UnitScalarConversion(LookUp.Get(9102), LookUp.Get(9103), 60), // degree to arc-minute
						new UnitScalarConversion(LookUp.Get(9103), LookUp.Get(9104), 60), // arc-minute to arc-second

						new UnitUnityConversion(LookUp.Get(9105), LookUp.Get(9106)), // grad and gon are the same
						new UnitRatioConversion(LookUp.Get(9105), LookUp.Get(9102), 9, 10), // grad to degree
						new UnitRatioConversion(LookUp.Get(9106), LookUp.Get(9102), 9, 10), // gon to degree
						new UnitScalarConversion(LookUp.Get(9105), LookUp.Get(9112), 100), // grad to centesimal minute
						new UnitScalarConversion(LookUp.Get(9106), LookUp.Get(9112), 100), // gon to centesimal minute

						new UnitScalarConversion(LookUp.Get(9112), LookUp.Get(9113), 100), // centesimal minute to centesimal second

						new SexagesimalDmsToDecimalDegreesConversion(LookUp.Get(9110), LookUp.Get(9102)), // sexagesimal dms to dd
						new SexagesimalDmToDecimalDegreesConversion(LookUp.Get(9111), LookUp.Get(9102)), // sexagesimal dm to dd 
					})
			);
		}

		private static void PopulateFromFile(string datFileName, string typeName, BinaryReader readerTxt, EpsgNumberLookUp numberLookUp, SortedDictionary<ushort, EpsgUnit> lookUpDictionary) {
			using (var readerDat = EpsgDataResource.CreateBinaryReader(datFileName)) {
				while (readerDat.BaseStream.Position < readerDat.BaseStream.Length) {
					var code = readerDat.ReadUInt16();
					var name = EpsgTextLookUp.GetString(readerDat.ReadUInt16(), readerTxt);
					var factorB = numberLookUp.Get(readerDat.ReadUInt16());
					var factorC = numberLookUp.Get(readerDat.ReadUInt16());
					lookUpDictionary.Add(code, new EpsgUnit(code, name, typeName, factorB, factorC));
				}
			}
		}

		public static EpsgUnit Get(int code) {
			return code >= 0 && code < ushort.MaxValue
				? LookUp.Get((ushort) code)
				: null;
		}

		public static IEnumerable<EpsgUnit> Values { get { return LookUp.Values; } }

		private readonly ushort _code;
		private readonly string _name;
		private readonly double _factorB;
		private readonly double _factorC;
		private readonly string _type;

		private EpsgUnit(ushort code, string name, string type, double factorB, double factorC) {
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

		private IUnitConversion<double> BuildConversionToBase() {
			var to = GetBaseUnit();
			// ReSharper disable CompareOfFloatsByEqualityOperator
			if(1.0 == FactorB && 1.0 == FactorC)
				return new UnitUnityConversion(this, to);
			if(1.0 != FactorB && 1.0 == FactorC)
				return new UnitScalarConversion(this, to, FactorB);
			if (0 == FactorB && 0 == FactorC)
				return null;
			return new UnitRatioConversion(this, to, FactorB, FactorC);
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		private IUnit GetBaseUnit() {
			if (StringComparer.OrdinalIgnoreCase.Equals("Angle", Type))
				return LookUp.Get(9101);
			if (StringComparer.OrdinalIgnoreCase.Equals("Length", Type))
				return LookUp.Get(9001);
			if (StringComparer.OrdinalIgnoreCase.Equals("Scale", Type))
				return LookUp.Get(9201);
			return null;
		}

		public IUnitConversionMap<double> ConversionMap { get { return AllConversionMap; }}

		public override string ToString(){
			return Name;
		}
	}
}
