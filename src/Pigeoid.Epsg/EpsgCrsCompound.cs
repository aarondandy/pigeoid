using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
    public class EpsgCrsCompound : EpsgCrs, ICrsCompound
    {

        internal EpsgCrsCompound(
            int code, string name, EpsgArea area, bool deprecated,
            EpsgCrsDatumBased horizontal, EpsgCrsVertical vertical
        )
            : base(code, name, area, deprecated) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(horizontal != null);
            Contract.Requires(vertical != null);
            Horizontal = horizontal;
            Vertical = vertical;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Horizontal != null);
            Contract.Invariant(Vertical != null);
        }

        public EpsgCrsDatumBased Horizontal { get; private set; }

        ICrs ICrsCompound.Head { get { return Horizontal; } }

        public EpsgCrsVertical Vertical { get; private set; }

        ICrs ICrsCompound.Tail { get { return Vertical; } }

        public override EpsgCrsKind Kind { get { return EpsgCrsKind.Compound; } }

    }
}
