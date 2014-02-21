using System.Diagnostics.Contracts;
using System.Globalization;
using DotSpatial.Projections;
using Pigeoid.Ogc;
using Pigeoid.Unit;

namespace Pigeoid.Interop.Proj4
{
    public class Proj4MeridianWrapper : OgcNamedAuthorityBoundEntity, IPrimeMeridianInfo
    {

        public Proj4MeridianWrapper(Meridian meridian)
            : base(meridian.Name, new AuthorityTag("PROJ4", meridian.Code.ToString(CultureInfo.InvariantCulture))) {
            Contract.Requires(meridian != null);
            Core = meridian;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Core != null);
        }

        protected Meridian Core { get; private set; }

        public double Longitude { get { return Core.Longitude; } }

        public IUnit Unit {
            get {
                return OgcAngularUnit.DefaultDegrees;
            }
        }
    }
}
