using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
    public class EpsgCrsVertical : EpsgCrsDatumBased, ICrsVertical
    {

        internal EpsgCrsVertical(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumVertical datum)
            : base(code, name, area, deprecated, cs) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(datum != null);
            VerticalDatum = datum;
        }

        private void CodeContractInvariants() {
            Contract.Invariant(VerticalDatum != null);
        }

        public override EpsgDatum Datum {
            get {
                Contract.Ensures(Contract.Result<EpsgDatum>() != null);
                return VerticalDatum;
            }
        }

        public EpsgDatumVertical VerticalDatum { get; private set; }

        IDatum ICrsVertical.Datum { get { return VerticalDatum; } }

        public EpsgUnit Unit {
            get {
                Contract.Ensures(Contract.Result<EpsgUnit>() != null);
                return Axis.Unit;
            }
        }

        IUnit ICrsVertical.Unit { get { return Unit; } }

        public EpsgAxis Axis {
            get {
                Contract.Ensures(Contract.Result<EpsgAxis>() != null);
                return CoordinateSystem.Axes.FirstOrDefault();
            }
        }

        IAxis ICrsVertical.Axis { get { return Axis; } }
    }
}
