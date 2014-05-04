using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using DotSpatial.Projections;
using Pigeoid.Ogc;
using Pigeoid.Unit;

namespace Pigeoid.Interop.Proj4
{
    public class Proj4MeridianWrapper : OgcNamedAuthorityBoundEntity, IPrimeMeridianInfo
    {

        public static Meridian Create(IPrimeMeridianInfo primeMeridian) {
            if(primeMeridian == null) throw new ArgumentNullException("primeMeridian");
            Contract.Ensures(Contract.Result<Meridian>() != null);

            var lon = primeMeridian.Longitude;
            if (primeMeridian.Unit.Name != "degrees") {
                var conversion = SimpleUnitConversionGenerator.FindConversion(primeMeridian.Unit, OgcAngularUnit.DefaultDegrees);
                if (conversion != null) {
                    lon = conversion.TransformValue(lon);
                }
                else {
                    throw new InvalidOperationException("Could not convert meridian unit.");
                }
            }

            var result = new Meridian(lon, primeMeridian.Name);
            
            if (primeMeridian.Authority != null) {
                int epsgCode;
                if (primeMeridian.Authority.Name == "EPSG" && Int32.TryParse(primeMeridian.Authority.Code, out epsgCode))
                    result.Code = epsgCode;
            }

            return result;
        }

        public Proj4MeridianWrapper(Meridian meridian)
            : base(meridian.Name ?? "Unknown", new AuthorityTag("PROJ4", meridian.Code.ToString(CultureInfo.InvariantCulture))) {
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
