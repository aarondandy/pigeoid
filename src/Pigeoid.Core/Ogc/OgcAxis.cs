using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
namespace Pigeoid.Ogc
{
    /// <summary>
    /// An axis of a coordinate system.
    /// </summary>
    public class OgcAxis : IAxis
    {

        /// <summary>
        /// Constructs a new axis.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <param name="ogcOrientationType">The direction of the axis.</param>
        public OgcAxis(string name, OgcOrientationType ogcOrientationType) {
            Name = name ?? String.Empty;
            OgcOrientationType = ogcOrientationType;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Name != null);
        }

        public string Name { get; private set; }

        public OgcOrientationType OgcOrientationType { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string IAxis.Orientation {
            get {
                return OgcOrientationType.ToString();
            }
        }
    }
}
