using System.Diagnostics.Contracts;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A prime meridian.
    /// </summary>
    public class OgcPrimeMeridian :
        OgcNamedAuthorityBoundEntity,
        IPrimeMeridianInfo
    {

        public static OgcPrimeMeridian DefaultGreenwich { get; private set; }

        static OgcPrimeMeridian() {
            DefaultGreenwich = new OgcPrimeMeridian("Greenwich", 0, new AuthorityTag("EPSG", "8901"));
        }

        /// <summary>
        /// Constructs a prime meridian.
        /// </summary>
        /// <param name="name">The name of the prime meridian.</param>
        /// <param name="longitude">The longitude location of the meridian.</param>
        /// <param name="authority">The authority.</param>
        public OgcPrimeMeridian(string name, double longitude, IAuthorityTag authority = null)
            : this(name, longitude, null, authority)
        {
            Contract.Requires(name != null);
        }

        /// <summary>
        /// Constructs a prime meridian.
        /// </summary>
        /// <param name="name">The name of the prime meridian.</param>
        /// <param name="longitude">The longitude location of the meridian.</param>
        /// <param name="angularUnit">The angular unit of the longitude value.</param>
        /// <param name="authority">The authority.</param>
        public OgcPrimeMeridian(string name, double longitude, IUnit angularUnit, IAuthorityTag authority = null)
            : base(name, authority)
        {
            Contract.Requires(name != null);
            Longitude = longitude;
            Unit = angularUnit ?? OgcAngularUnit.DefaultDegrees;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Unit != null);
        }

        public double Longitude { get; private set; }

        public IUnit Unit { get; private set; }

    }
}
