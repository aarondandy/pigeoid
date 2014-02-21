using System;
using System.Diagnostics.Contracts;
using DotSpatial.Projections;
using Pigeoid.Ogc;
using Pigeoid.Unit;

namespace Pigeoid.Interop.Proj4
{
    public class Proj4SpheroidWrapper : OgcNamedAuthorityBoundEntity, ISpheroidInfo
    {

        public Proj4SpheroidWrapper(Spheroid spheroid)
            : base(spheroid.Name, new AuthorityTag("PROJ4", spheroid.Code)) {
            Contract.Requires(spheroid != null);
            Core = spheroid;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Core != null);
        }

        protected Spheroid Core { get; private set; }

        public IUnit AxisUnit { get { return OgcLinearUnit.DefaultMeter; } }

        public double A {
            get { return Core.EquatorialRadius; }
        }

        public double B {
            get { return Core.PolarRadius; }
        }

        public double E {
            get { return Math.Sqrt(2.0 * InvF - 1.0) / InvF; }
        }

        public double ESecond {
            get { return Math.Sqrt(2.0 * InvF - 1.0) / (InvF - 1.0); }
        }

        public double ESecondSquared {
            get {
                var num = InvF - 1.0;
                return (2.0 * InvF - 1.0) / (num * num);
            }
        }

        public double ESquared {
            get {
                return (2.0 * InvF - 1.0) / (InvF * InvF);
            }
        }

        public double F {
            get { return 1.0 / InvF; }
        }

        public double InvF {
            get { return Core.InverseFlattening; }
        }
    }
}
