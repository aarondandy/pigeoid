// TODO: source header

using System.Collections.Generic;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgEllipsoid : ISpheroid<double>
	{

		internal class EpsgEllipsoidLookup : EpsgFixedLookupBase<ushort, EpsgEllipsoid>
		{
			private static SortedDictionary<ushort, EpsgEllipsoid> GenerateLookup() {
				var lookup = new SortedDictionary<ushort, EpsgEllipsoid>();
				using (var readerTxt = EpsgDataResource.CreateBinaryReader("ellipsoids.txt"))
				using (var numberLookup = new EpsgNumberLookup())
				using (var readerDat = EpsgDataResource.CreateBinaryReader("ellipsoids.dat")) {
					for (int i = readerDat.ReadUInt16(); i > 0; i--) {
						var code = readerDat.ReadUInt16();
						var semiMajorAxis = numberLookup.Get(readerDat.ReadUInt16());
						var valueB = numberLookup.Get(readerDat.ReadUInt16());
						var name = EpsgTextLookup.GetString(readerDat.ReadUInt16(), readerTxt);
						var uom = EpsgUom.Get(readerDat.ReadByte() + 9000);
						lookup.Add(code, new EpsgEllipsoid(code, name, CreateSpheroid(semiMajorAxis, valueB), uom));
					}
				}
				return lookup;
			}

			private static ISpheroid<double> CreateSpheroid(double semiMajorAxis, double valueB) {
				if (valueB < semiMajorAxis / 10.0)
					return new SpheroidEquatorialInvF(semiMajorAxis, valueB);
				if (valueB == semiMajorAxis)
					return new Sphere(semiMajorAxis);
				return new SpheroidEquatorialPolar(semiMajorAxis, valueB);
			}

			public EpsgEllipsoidLookup() : base(GenerateLookup()) { }

		}

		internal static readonly EpsgEllipsoidLookup Lookup = new EpsgEllipsoidLookup();

		public static EpsgEllipsoid Get(int code) {
			return Lookup.Get(checked((ushort)code));
		}

		public static IEnumerable<EpsgEllipsoid> Values { get { return Lookup.Values; } }

		private readonly ushort _code;
		private readonly ISpheroid<double> _core;
		private readonly string _name;
		private readonly EpsgUom _uom;

		private EpsgEllipsoid(ushort code, string name, ISpheroid<double> core, EpsgUom uom) {
			_code = code;
			_name = name;
			_core = core;
			_uom = uom;
		}

		public int Code { get { return _code; } }

		public string Name { get { return _name; } }

		public EpsgUom Unit { get { return _uom; } }

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
