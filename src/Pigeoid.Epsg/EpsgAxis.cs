using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;
using Pigeoid.Epsg.Utility;

namespace Pigeoid.Epsg
{
    public class EpsgAxis : IAxis
    {

        [Obsolete]
        private static readonly EpsgDataResourceAllAxisSetReaders AllReaders = new EpsgDataResourceAllAxisSetReaders();

        [Obsolete]
        internal static EpsgAxis[] Get(ushort csCode) {
            Contract.Ensures(Contract.Result<EpsgAxis[]>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<EpsgAxis[]>(), x => x != null));
            var set = AllReaders.GetSetByCsKey(csCode);
            if (set == null)
                return ArrayUtil<EpsgAxis>.Empty;
            return set.Axes.ToArray();
        }

        internal EpsgAxis(string name, string abbreviation, string orientation, EpsgUnit unit) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(!String.IsNullOrEmpty(abbreviation));
            Contract.Requires(!String.IsNullOrEmpty(orientation));
            Contract.Requires(unit != null);
            Name = name;
            Abbreviation = abbreviation;
            Orientation = orientation;
            Unit = unit;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(!String.IsNullOrEmpty(Abbreviation));
            Contract.Invariant(!String.IsNullOrEmpty(Orientation));
            Contract.Invariant(Unit != null);
        }

        public EpsgUnit Unit { get; private set; }

        public string Name { get; private set; }

        public string Abbreviation { get; private set; }

        public string Orientation { get; private set; }

    }
}
