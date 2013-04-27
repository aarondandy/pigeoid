using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// An angular unit of measure.
    /// </summary>
    public class OgcAngularUnit : OgcUnitBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly OgcAngularUnit _defaultGrads;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly OgcAngularUnit _defaultArcSeconds;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly OgcAngularUnit _defaultDegrees;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly OgcAngularUnit _defaultRadians;

        static OgcAngularUnit() {
            _defaultRadians = new OgcAngularUnit("radian", 1, new AuthorityTag("EPSG", "9101"));
            _defaultDegrees = new OgcAngularUnit("degree", Math.PI / 180.0, new AuthorityTag("EPSG", "9102"));
            _defaultGrads = new OgcAngularUnit("grad", Math.PI / 200.0, new AuthorityTag("EPSG", "9105"));
            _defaultArcSeconds = new OgcAngularUnit("arc-second", Math.PI / 648000.0, new AuthorityTag("EPSG", "9104"));
        }

        /// <summary>
        /// This is the OGC reference unit for angular measure.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static OgcAngularUnit DefaultRadians {
            get {
                Contract.Ensures(Contract.Result<OgcAngularUnit>() != null);
                return _defaultRadians;
            }
        }

        /// <summary>
        /// The default degree unit factored against radians.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static OgcAngularUnit DefaultDegrees {
            get {
                Contract.Ensures(Contract.Result<OgcAngularUnit>() != null);
                return _defaultDegrees;
            }
        }

        /// <summary>
        /// The default arc-second unit factored against radians.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static OgcAngularUnit DefaultArcSeconds {
            get {
                Contract.Ensures(Contract.Result<OgcAngularUnit>() != null);
                return _defaultArcSeconds;
            }
        }

        /// <summary>
        /// The default grad unit factored against radians.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static OgcAngularUnit DefaultGrads {
            get {
                Contract.Ensures(Contract.Result<OgcAngularUnit>() != null);
                return _defaultGrads;
            }
        }

        /// <summary>
        /// Constructs a new unit.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="factor">The conversion factor to the base unit.</param>
        public OgcAngularUnit(string name, double factor)
            : base(name, factor) {Contract.Requires(name != null);}

        /// <summary>
        /// Constructs a new unit.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="factor">The conversion factor to the base unit.</param>
        /// <param name="authority">The authority.</param>
        public OgcAngularUnit(string name, double factor, IAuthorityTag authority)
            : base(name, factor, authority) {Contract.Requires(name != null);}

        public override string Type { get { return "angle"; } }

        public override IUnit ReferenceUnit {
            get { return DefaultRadians; }
        }

    }
}
