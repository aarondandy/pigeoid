using System;
using System.Diagnostics.Contracts;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A datum.
    /// </summary>
    public class OgcDatum : OgcNamedAuthorityBoundEntity, IDatum
    {

        /// <summary>
        /// Constructs a local datum.
        /// </summary>
        /// <param name="name">The datum name.</param>
        /// <param name="type">The datum type code.</param>
        /// <param name="authority">The authority.</param>
        public OgcDatum(string name, OgcDatumType type, IAuthorityTag authority)
            : base(name, authority)
        {
            Contract.Requires(name != null);
            OgcType = type;
        }

        public OgcDatumType OgcType { get; private set; }

        public string Type {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                switch (OgcType) {
                    case OgcDatumType.LocalOther: return "Local";
                    case OgcDatumType.VerticalOther: return "Vertical";
                    case OgcDatumType.HorizontalOther: return "Horizontal";
                    default: return OgcType.ToString();
                }
            }
        }

    }
}
