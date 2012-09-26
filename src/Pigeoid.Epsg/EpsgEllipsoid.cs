// TODO: source header

using System.Collections.Generic;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgEllipsoid : ISpheroidInfo
	{

		internal static readonly EpsgFixedLookUpBase<ushort, EpsgEllipsoid> LookUp;

		static EpsgEllipsoid() {
			var lookUpDictionary = new SortedDictionary<ushort, EpsgEllipsoid>();
			using (var readerTxt = EpsgDataResource.CreateBinaryReader("ellipsoids.txt"))
			using (var numberLookUp = new EpsgNumberLookUp())
			using (var readerDat = EpsgDataResource.CreateBinaryReader("ellipsoids.dat")) {
				for (int i = readerDat.ReadUInt16(); i > 0; i--) {
					var code = readerDat.ReadUInt16();
					var semiMajorAxis = numberLookUp.Get(readerDat.ReadUInt16());
					var valueB = numberLookUp.Get(readerDat.ReadUInt16());
					var name = EpsgTextLookUp.GetString(readerDat.ReadUInt16(), readerTxt);
					var uom = EpsgUom.Get(readerDat.ReadByte() + 9000);
// ReSharper disable CompareOfFloatsByEqualityOperator
					lookUpDictionary.Add(code, new EpsgEllipsoid(
						code, name, uom,
						(valueB == semiMajorAxis)
							? new Sphere(semiMajorAxis)
						: (valueB < semiMajorAxis / 10.0)
							? new SpheroidEquatorialInvF(semiMajorAxis, valueB) as ISpheroid<double>
						: new SpheroidEquatorialPolar(semiMajorAxis, valueB)
					));
// ReSharper restore CompareOfFloatsByEqualityOperator
				}
			}
			LookUp = new EpsgFixedLookUpBase<ushort, EpsgEllipsoid>(lookUpDictionary);
		}

		public static EpsgEllipsoid Get(int code) {
			return code >= 0 && code < ushort.MaxValue
				? LookUp.Get((ushort) code)
				: null;
		}

		public static IEnumerable<EpsgEllipsoid> Values { get { return LookUp.Values; } }

		private readonly ushort _code;
		private readonly ISpheroid<double> _core;
		private readonly string _name;
		private readonly EpsgUom _uom;

		private EpsgEllipsoid(ushort code, string name, EpsgUom uom, ISpheroid<double> core) {
			_code = code;
			_name = name;
			_core = core;
			_uom = uom;
		}

		public int Code { get { return _code; } }

		public string Name { get { return _name; } }

		public EpsgUom AxisUnit { get { return _uom; } }

		IUom ISpheroidInfo.AxisUnit { get { return AxisUnit; } }

		public ISpheroid<double> Core { get { return _core; } }

		public double A {
			get { return _core.A; }
		}

		public double B {
			get { return _core.B; }
		}

		public double E {
			get { return _core.E; }
		}

		public double ESecond {
			get { return _core.ESecond; }
		}

		public double ESecondSquared {
			get { return _core.ESecondSquared; }
		}

		public double ESquared {
			get { return _core.ESquared; }
		}

		public double F {
			get { return _core.F; }
		}

		public double InvF {
			get { return _core.InvF; }
		}

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

	}
}
