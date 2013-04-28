using System.Diagnostics;
using System.Diagnostics.Contracts;
using Pigeoid.Unit;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// A linear unit of measure.
    /// </summary>
    public class OgcLinearUnit : OgcUnitBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly OgcLinearUnit _defaultMeter;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly OgcLinearUnit _defaultKilometer;

        static OgcLinearUnit() {
            _defaultMeter = new OgcLinearUnit("Meter", 1, new AuthorityTag("EPSG", "9001"));
            _defaultKilometer = new OgcLinearUnit("Kilometer", 1000, new AuthorityTag("EPSG", "9036"));
        }

        /// <summary>
        /// The default OGC reference unit for length measures.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static OgcLinearUnit DefaultMeter {
            get {
                Contract.Ensures(Contract.Result<OgcLinearUnit>() != null);
                return _defaultMeter;
            }
        }

        /// <summary>
        /// The default kilometer unit.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static OgcLinearUnit DefaultKilometer {
            get {
                Contract.Ensures(Contract.Result<OgcLinearUnit>() != null);
                return _defaultKilometer;
            }
        }

        /// <summary>
        /// Constructs a new unit.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="factor">The conversion factor to the base unit.</param>
        public OgcLinearUnit(string name, double factor)
            : base(name, factor)
        {
            Contract.Requires(name != null);
        }

        /// <summary>
        /// Constructs a new unit.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="factor">The conversion factor to the base unit.</param>
        /// <param name="authority">The authority.</param>
        public OgcLinearUnit(string name, double factor, IAuthorityTag authority)
            : base(name, factor, authority)
        {
            Contract.Requires(name != null);
        }

        public override string Type {
            get { return "length"; }
        }

        public override IUnit ReferenceUnit {
            get { return DefaultMeter; }
        }

    }
}
