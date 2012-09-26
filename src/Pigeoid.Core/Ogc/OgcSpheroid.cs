// TODO: source header

using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A spheroid.
	/// </summary>
	public class OgcSpheroid : OgcNamedAuthorityBoundEntity, ISpheroidInfo
	{
		/// <summary>
		/// The spheroid data this OGC spheroid is based on.
		/// </summary>
		public readonly ISpheroid<double> Spheroid;

		/// <summary>
		/// Constructs a new spheroid.
		/// </summary>
		/// <param name="spheroid">The spheroid this spheroid is based on.</param>
		/// <param name="name">The name of this spheroid.</param>
		/// <param name="axisUnit">The unit the axis is measured in.</param>
		/// <param name="authority">The authority.</param>
		public OgcSpheroid(ISpheroid<double> spheroid, string name, IUom axisUnit, IAuthorityTag authority = null)
			: base(name, authority) {
			Spheroid = spheroid;
			AxisUnit = axisUnit;
		}

		public IUom AxisUnit { get; private set; }

		public double A {
			get { return Spheroid.A; }
		}

		public double B {
			get { return Spheroid.B; }
		}

		double ISpheroid<double>.F {
			get { return Spheroid.F; }
		}

		public double InvF {
			get { return Spheroid.InvF; }
		}

		double ISpheroid<double>.E {
			get { return Spheroid.E; }
		}

		double ISpheroid<double>.ESquared {
			get { return Spheroid.ESquared; }
		}

		double ISpheroid<double>.ESecond {
			get { return Spheroid.ESecond; }
		}

		double ISpheroid<double>.ESecondSquared {
			get { return Spheroid.ESecondSquared; }
		}

	}
}
