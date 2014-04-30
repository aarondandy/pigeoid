using System;
using System.Diagnostics.Contracts;
using Pigeoid.Unit;
using Vertesaur;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A spheroid.
    /// </summary>
    public class OgcSpheroid : OgcNamedAuthorityBoundEntity, ISpheroidInfo
    {
        private static OgcSpheroid _defaultWgs84;

        public static OgcSpheroid DefaultWgs84 {
            get {
                Contract.Ensures(Contract.Result<OgcSpheroid>() != null);
                return _defaultWgs84;
            }
        }

        static OgcSpheroid() {
            _defaultWgs84 = new OgcSpheroid(
                new SpheroidEquatorialInvF(6378137, 298.257223563),
                "WGS 84",
                OgcLinearUnit.DefaultMeter,
                new AuthorityTag("EPSG", "7030")
            );
        }

        /// <summary>
        /// Constructs a new spheroid.
        /// </summary>
        /// <param name="spheroid">The spheroid this spheroid is based on.</param>
        /// <param name="name">The name of this spheroid.</param>
        /// <param name="axisUnit">The unit the axis is measured in.</param>
        /// <param name="authority">The authority.</param>
        public OgcSpheroid(ISpheroid<double> spheroid, string name, IUnit axisUnit, IAuthorityTag authority = null)
            : base(name, authority)
        {
            if(spheroid == null) throw new ArgumentNullException("spheroid");
            Contract.Requires(name != null);
            Spheroid = spheroid;
            AxisUnit = axisUnit;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Spheroid != null);
        }

        /// <summary>
        /// The spheroid data this OGC spheroid is based on.
        /// </summary>
        public ISpheroid<double> Spheroid { get; private set; }

        public IUnit AxisUnit { get; private set; }

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

        public override string ToString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var result = "MajorAxis: " + A + " InverseF: " + B;
            if (!String.IsNullOrEmpty(Name))
                result += " (" + Name + ')';
            Contract.Assume(!String.IsNullOrEmpty(result));
            return result;
        }
    }
}
