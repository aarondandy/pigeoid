using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;
using System.Collections.Generic;
using System;

namespace Pigeoid.Epsg
{
    public abstract class EpsgCrsDatumBased : EpsgCrs
    {

        internal EpsgCrsDatumBased(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs)
            : base(code, name, area, deprecated) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            CoordinateSystem = cs;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(CoordinateSystem != null);
        }

        public EpsgCoordinateSystem CoordinateSystem { get; private set; }

        public abstract EpsgDatum Datum { get; }

        public abstract override EpsgCrsKind Kind { get; }

    }
}
