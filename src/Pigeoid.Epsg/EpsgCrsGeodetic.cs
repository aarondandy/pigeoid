using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
    public class EpsgCrsGeodetic : EpsgCrsDatumBased, ICrsGeodetic
    {

        internal EpsgCrsGeodetic(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
            : base(code, name, area, deprecated, cs) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
            GeodeticDatum = geodeticDatum;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(GeodeticDatum != null);
        }

        public override EpsgDatum Datum {
            get {
                Contract.Ensures(Contract.Result<EpsgDatum>() != null);
                return GeodeticDatum;
            }
        }

        public EpsgDatumGeodetic GeodeticDatum { get; private set; }

        IDatumGeodetic ICrsGeodetic.Datum { get { return GeodeticDatum; } }

        public EpsgUnit Unit {
            get {
                Contract.Ensures(Contract.Result<EpsgUnit>() != null);
                return CoordinateSystem.Axes.First().Unit;
            }
        }

        IUnit ICrsGeodetic.Unit { get { return Unit; } }

        public IList<EpsgAxis> Axes {
            get {
                Contract.Ensures(Contract.Result<IList<EpsgAxis>>() != null);
                return CoordinateSystem.Axes.ToArray();
            }
        }

        IList<IAxis> ICrsGeodetic.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }
    }

    public class EpsgCrsGeocentric : EpsgCrsGeodetic, ICrsGeocentric
    {
        internal EpsgCrsGeocentric(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
            : base(code, name, area, deprecated, cs, geodeticDatum) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
        }
    }

    public class EpsgCrsGeographic : EpsgCrsGeodetic, ICrsGeographic
    {
        internal EpsgCrsGeographic(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
            : base(code, name, area, deprecated, cs, geodeticDatum) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
        }
    }

}
