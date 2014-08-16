using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    internal class EpsgAxisSet
    {

        internal EpsgAxisSet(ushort key, EpsgAxis[] axes) {
            Contract.Requires(axes != null);
            Contract.Requires(Contract.ForAll(axes, x => x != null));
            CoordinateSystemKey = key;
            Axes = axes;
        }

        public ushort CoordinateSystemKey { get; private set; }
        public EpsgAxis[] Axes { get; private set; }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Axes != null);
            Contract.Invariant(Contract.ForAll(Axes, x => x != null));
        }

    }
}
