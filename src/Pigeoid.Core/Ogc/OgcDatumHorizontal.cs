using System;
using System.Diagnostics.Contracts;
using Pigeoid.Contracts;
using Pigeoid.Transformation;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A horizontal or geodetic datum.
    /// </summary>
    public class OgcDatumHorizontal : OgcDatum, IDatumGeodetic
    {

        public static readonly OgcDatumHorizontal DefaultWgs84 = new OgcDatumHorizontal(
            "WGS_1984",
            OgcSpheroid.DefaultWgs84,
            OgcPrimeMeridian.DefaultGreenwich,
            Helmert7Transformation.IdentityTransformation,
            new AuthorityTag("EPSG", "6326")
        );

        /// <summary>
        /// Constructs a horizontal datum.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="spheroid">The spheroid of the datum.</param>
        /// <param name="primeMeridian">The prime meridian of the datum.</param>
        /// <param name="transform">The transformation for conversions to WGS84.</param>
        /// <param name="authority">The authority.</param>
        public OgcDatumHorizontal(
            string name,
            ISpheroidInfo spheroid,
            IPrimeMeridianInfo primeMeridian,
            Helmert7Transformation transform,
            IAuthorityTag authority = null
        )
            : base(name, OgcDatumType.None, authority)
        {
            if (null == spheroid) throw new ArgumentNullException("spheroid");
            Contract.Requires(name != null);
            Spheroid = spheroid;
            PrimeMeridian = primeMeridian;
            BasicWgs84Transformation = transform;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Spheroid != null);
            Contract.Invariant(!IsTransformableToWgs84 || BasicWgs84Transformation != null); // if IsTransformableToWgs84 then BasicWgs84Transformation != null else ignore
        }

        public IPrimeMeridianInfo PrimeMeridian { get; private set; }

        public ISpheroidInfo Spheroid { get; private set; }

        public bool IsTransformableToWgs84 { get { return BasicWgs84Transformation != null; } }

        public Helmert7Transformation BasicWgs84Transformation { get; private set; }


    }
}
